﻿using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Vulkan
{
    public unsafe static class VulkanInitialization
    {
        private const uint InvalidIndex = uint.MaxValue;
        private static uint MinimalVulkanVersion = Vk.Version11.Value;
        private static uint MinimalInstanceVulkanVersion = Vk.Version12.Value;
        private static uint MaximumVulkanVersion = Vk.Version12.Value;
        private const string AppName = "Ryujinx.Graphics.Vulkan";
        private const int QueuesCount = 2;

        private static readonly string[] _desirableExtensions = new string[]
        {
            ExtConditionalRendering.ExtensionName,
            ExtExtendedDynamicState.ExtensionName,
            ExtTransformFeedback.ExtensionName,
            KhrDrawIndirectCount.ExtensionName,
            KhrPushDescriptor.ExtensionName,
            "VK_EXT_blend_operation_advanced",
            "VK_EXT_custom_border_color",
            "VK_EXT_descriptor_indexing", // Enabling this works around an issue with disposed buffer bindings on RADV.
            "VK_EXT_fragment_shader_interlock",
            "VK_EXT_index_type_uint8",
            "VK_EXT_primitive_topology_list_restart",
            "VK_EXT_robustness2",
            "VK_EXT_shader_stencil_export",
            "VK_KHR_shader_float16_int8",
            "VK_EXT_shader_subgroup_ballot",
            "VK_EXT_subgroup_size_control",
            "VK_NV_geometry_shader_passthrough",
            "VK_KHR_portability_subset", // By spec, we should enable this if present.
        };

        private static readonly string[] _requiredExtensions = new string[]
        {
            KhrSwapchain.ExtensionName
        };

        internal static Instance CreateInstance(Vk api, GraphicsDebugLevel logLevel, string[] requiredExtensions)
        {
            var enabledLayers = new List<string>();

            void AddAvailableLayer(string layerName)
            {
                uint layerPropertiesCount;

                api.EnumerateInstanceLayerProperties(&layerPropertiesCount, null).ThrowOnError();

                LayerProperties[] layerProperties = new LayerProperties[layerPropertiesCount];

                fixed (LayerProperties* pLayerProperties = layerProperties)
                {
                    api.EnumerateInstanceLayerProperties(&layerPropertiesCount, layerProperties).ThrowOnError();

                    for (int i = 0; i < layerPropertiesCount; i++)
                    {
                        string currentLayerName = Marshal.PtrToStringAnsi((IntPtr)pLayerProperties[i].LayerName);

                        if (currentLayerName == layerName)
                        {
                            enabledLayers.Add(layerName);
                            return;
                        }
                    }
                }

                Logger.Warning?.Print(LogClass.Gpu, $"Missing layer {layerName}");
            }

            if (logLevel != GraphicsDebugLevel.None)
            {
                AddAvailableLayer("VK_LAYER_KHRONOS_validation");
            }

            var enabledExtensions = requiredExtensions;

            if (api.IsInstanceExtensionPresent("VK_EXT_debug_utils"))
            {
                enabledExtensions = enabledExtensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

            var appName = Marshal.StringToHGlobalAnsi(AppName);

            var applicationInfo = new ApplicationInfo
            {
                PApplicationName = (byte*)appName,
                ApplicationVersion = 1,
                PEngineName = (byte*)appName,
                EngineVersion = 1,
                ApiVersion = MaximumVulkanVersion
            };

            IntPtr* ppEnabledExtensions = stackalloc IntPtr[enabledExtensions.Length];
            IntPtr* ppEnabledLayers = stackalloc IntPtr[enabledLayers.Count];

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                ppEnabledExtensions[i] = Marshal.StringToHGlobalAnsi(enabledExtensions[i]);
            }

            for (int i = 0; i < enabledLayers.Count; i++)
            {
                ppEnabledLayers[i] = Marshal.StringToHGlobalAnsi(enabledLayers[i]);
            }

            var instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = (byte**)ppEnabledExtensions,
                PpEnabledLayerNames = (byte**)ppEnabledLayers,
                EnabledExtensionCount = (uint)enabledExtensions.Length,
                EnabledLayerCount = (uint)enabledLayers.Count
            };

            api.CreateInstance(in instanceCreateInfo, null, out var instance).ThrowOnError();

            Marshal.FreeHGlobal(appName);

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                Marshal.FreeHGlobal(ppEnabledExtensions[i]);
            }

            for (int i = 0; i < enabledLayers.Count; i++)
            {
                Marshal.FreeHGlobal(ppEnabledLayers[i]);
            }

            return instance;
        }

        internal static VulkanPhysicalDevice FindSuitablePhysicalDevice(Vk api, Instance instance, SurfaceKHR surface, string preferredGpuId)
        {
            uint physicalDeviceCount;

            api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, null).ThrowOnError();

            PhysicalDevice[] physicalDevices = new PhysicalDevice[physicalDeviceCount];

            fixed (PhysicalDevice* pPhysicalDevices = physicalDevices)
            {
                api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, pPhysicalDevices).ThrowOnError();
            }

            // First we try to pick the the user preferred GPU.
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                VulkanPhysicalDevice physicalDevice = new VulkanPhysicalDevice(api, physicalDevices[i]);

                if (IsPreferredAndSuitableDevice(api, physicalDevice, surface, preferredGpuId))
                {
                    return physicalDevice;
                }
            }

            // If we fail to do that, just use the first compatible GPU.
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                VulkanPhysicalDevice physicalDevice = new VulkanPhysicalDevice(api, physicalDevices[i]);

                if (IsSuitableDevice(api, physicalDevice, surface))
                {
                    return physicalDevice;
                }
            }

            throw new VulkanException("Initialization failed, none of the available GPUs meets the minimum requirements.");
        }

        internal static DeviceInfo[] GetSuitablePhysicalDevices(Vk api)
        {
            var appName = Marshal.StringToHGlobalAnsi(AppName);

            var applicationInfo = new ApplicationInfo
            {
                PApplicationName = (byte*)appName,
                ApplicationVersion = 1,
                PEngineName = (byte*)appName,
                EngineVersion = 1,
                ApiVersion = MaximumVulkanVersion
            };

            var instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = null,
                PpEnabledLayerNames = null,
                EnabledExtensionCount = 0,
                EnabledLayerCount = 0
            };

            api.CreateInstance(in instanceCreateInfo, null, out var instance).ThrowOnError();

            // We ensure that vkEnumerateInstanceVersion is present (added in 1.1).
            // If the instance doesn't support it, no device is going to be 1.1 compatible.
            if (api.GetInstanceProcAddr(instance, "vkEnumerateInstanceVersion") == IntPtr.Zero)
            {
                api.DestroyInstance(instance, null);

                return Array.Empty<DeviceInfo>();
            }

            // We currently assume that the instance is compatible with Vulkan 1.2
            // TODO: Remove this once we relax our initialization codepaths.
            uint instanceApiVerison = 0;
            api.EnumerateInstanceVersion(ref instanceApiVerison).ThrowOnError();

            if (instanceApiVerison < MinimalInstanceVulkanVersion)
            {
                api.DestroyInstance(instance, null);

                return Array.Empty<DeviceInfo>();
            }

            Marshal.FreeHGlobal(appName);

            uint physicalDeviceCount;

            api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, null).ThrowOnError();

            PhysicalDevice[] physicalDevices = new PhysicalDevice[physicalDeviceCount];

            fixed (PhysicalDevice* pPhysicalDevices = physicalDevices)
            {
                api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, pPhysicalDevices).ThrowOnError();
            }

            DeviceInfo[] devices = new DeviceInfo[physicalDevices.Length];

            for (int i = 0; i < physicalDevices.Length; i++)
            {
                VulkanPhysicalDevice physicalDevice = new VulkanPhysicalDevice(api, physicalDevices[i]);

                devices[i] = physicalDevice.ToDeviceInfo();
            }

            api.DestroyInstance(instance, null);

            return devices;
        }

        private static bool IsPreferredAndSuitableDevice(Vk api, VulkanPhysicalDevice physicalDevice, SurfaceKHR surface, string preferredGpuId)
        {
            if (physicalDevice.Id != preferredGpuId)
            {
                return false;
            }

            return IsSuitableDevice(api, physicalDevice, surface);
        }

        private static bool IsSuitableDevice(Vk api, VulkanPhysicalDevice physicalDevice, SurfaceKHR surface)
        {
            int extensionMatches = 0;

            foreach (string requiredExtension in _requiredExtensions)
            {
                if (physicalDevice.IsDeviceExtensionPresent(requiredExtension))
                {
                    extensionMatches++;
                }
            }

            return extensionMatches == _requiredExtensions.Length && FindSuitableQueueFamily(api, physicalDevice, surface, out _) != InvalidIndex;
        }

        internal static uint FindSuitableQueueFamily(Vk api, VulkanPhysicalDevice physicalDevice, SurfaceKHR surface, out uint queueCount)
        {
            const QueueFlags RequiredFlags = QueueFlags.GraphicsBit | QueueFlags.ComputeBit;

            var khrSurface = new KhrSurface(api.Context);

            for (uint index = 0; index < physicalDevice.QueueFamilyProperties.Length; index++)
            {
                ref QueueFamilyProperties property = ref physicalDevice.QueueFamilyProperties[index];

                khrSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice.PhysicalDevice, index, surface, out var surfaceSupported).ThrowOnError();

                if (property.QueueFlags.HasFlag(RequiredFlags) && surfaceSupported)
                {
                    queueCount = property.QueueCount;

                    return index;
                }
            }

            queueCount = 0;

            return InvalidIndex;
        }

        internal static Device CreateDevice(Vk api, VulkanPhysicalDevice physicalDevice, uint queueFamilyIndex, uint queueCount)
        {
            if (queueCount > QueuesCount)
            {
                queueCount = QueuesCount;
            }

            float* queuePriorities = stackalloc float[(int)queueCount];

            for (int i = 0; i < queueCount; i++)
            {
                queuePriorities[i] = 1f;
            }

            var queueCreateInfo = new DeviceQueueCreateInfo()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queueFamilyIndex,
                QueueCount = queueCount,
                PQueuePriorities = queuePriorities
            };

            bool useRobustBufferAccess = VendorUtils.FromId(physicalDevice.PhysicalDeviceProperties.VendorID) == Vendor.Nvidia;

            PhysicalDeviceFeatures2 features2 = new PhysicalDeviceFeatures2()
            {
                SType = StructureType.PhysicalDeviceFeatures2
            };

            PhysicalDeviceVulkan11Features supportedFeaturesVk11 = new PhysicalDeviceVulkan11Features()
            {
                SType = StructureType.PhysicalDeviceVulkan11Features,
                PNext = features2.PNext
            };

            features2.PNext = &supportedFeaturesVk11;

            PhysicalDeviceCustomBorderColorFeaturesEXT supportedFeaturesCustomBorderColor = new PhysicalDeviceCustomBorderColorFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceCustomBorderColorFeaturesExt,
                PNext = features2.PNext
            };

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_custom_border_color"))
            {
                features2.PNext = &supportedFeaturesCustomBorderColor;
            }

            PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT supportedFeaturesPrimitiveTopologyListRestart = new PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT()
            {
                SType = StructureType.PhysicalDevicePrimitiveTopologyListRestartFeaturesExt,
                PNext = features2.PNext
            };

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_primitive_topology_list_restart"))
            {
                features2.PNext = &supportedFeaturesPrimitiveTopologyListRestart;
            }

            PhysicalDeviceTransformFeedbackFeaturesEXT supportedFeaturesTransformFeedback = new PhysicalDeviceTransformFeedbackFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceTransformFeedbackFeaturesExt,
                PNext = features2.PNext
            };

            if (physicalDevice.IsDeviceExtensionPresent(ExtTransformFeedback.ExtensionName))
            {
                features2.PNext = &supportedFeaturesTransformFeedback;
            }

            PhysicalDeviceRobustness2FeaturesEXT supportedFeaturesRobustness2 = new PhysicalDeviceRobustness2FeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceRobustness2FeaturesExt
            };

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_robustness2"))
            {
                supportedFeaturesRobustness2.PNext = features2.PNext;

                features2.PNext = &supportedFeaturesRobustness2;
            }

            api.GetPhysicalDeviceFeatures2(physicalDevice.PhysicalDevice, &features2);

            var supportedFeatures = features2.Features;

            var features = new PhysicalDeviceFeatures()
            {
                DepthBiasClamp = true,
                DepthClamp = supportedFeatures.DepthClamp,
                DualSrcBlend = supportedFeatures.DualSrcBlend,
                FragmentStoresAndAtomics = true,
                GeometryShader = supportedFeatures.GeometryShader,
                ImageCubeArray = true,
                IndependentBlend = true,
                LogicOp = supportedFeatures.LogicOp,
                OcclusionQueryPrecise = supportedFeatures.OcclusionQueryPrecise,
                MultiViewport = supportedFeatures.MultiViewport,
                PipelineStatisticsQuery = supportedFeatures.PipelineStatisticsQuery,
                SamplerAnisotropy = true,
                ShaderClipDistance = true,
                ShaderFloat64 = supportedFeatures.ShaderFloat64,
                ShaderImageGatherExtended = supportedFeatures.ShaderImageGatherExtended,
                ShaderStorageImageMultisample = supportedFeatures.ShaderStorageImageMultisample,
                // ShaderStorageImageReadWithoutFormat = true,
                // ShaderStorageImageWriteWithoutFormat = true,
                TessellationShader = supportedFeatures.TessellationShader,
                VertexPipelineStoresAndAtomics = true,
                RobustBufferAccess = useRobustBufferAccess
            };

            void* pExtendedFeatures = null;

            PhysicalDeviceTransformFeedbackFeaturesEXT featuresTransformFeedback;

            if (physicalDevice.IsDeviceExtensionPresent(ExtTransformFeedback.ExtensionName))
            {
                featuresTransformFeedback = new PhysicalDeviceTransformFeedbackFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceTransformFeedbackFeaturesExt,
                    PNext = pExtendedFeatures,
                    TransformFeedback = supportedFeaturesTransformFeedback.TransformFeedback
                };

                pExtendedFeatures = &featuresTransformFeedback;
            }

            PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT featuresPrimitiveTopologyListRestart;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_primitive_topology_list_restart"))
            {
                featuresPrimitiveTopologyListRestart = new PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT()
                {
                    SType = StructureType.PhysicalDevicePrimitiveTopologyListRestartFeaturesExt,
                    PNext = pExtendedFeatures,
                    PrimitiveTopologyListRestart = supportedFeaturesPrimitiveTopologyListRestart.PrimitiveTopologyListRestart,
                    PrimitiveTopologyPatchListRestart = supportedFeaturesPrimitiveTopologyListRestart.PrimitiveTopologyPatchListRestart
                };

                pExtendedFeatures = &featuresPrimitiveTopologyListRestart;
            }

            PhysicalDeviceRobustness2FeaturesEXT featuresRobustness2;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_robustness2"))
            {
                featuresRobustness2 = new PhysicalDeviceRobustness2FeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceRobustness2FeaturesExt,
                    PNext = pExtendedFeatures,
                    NullDescriptor = supportedFeaturesRobustness2.NullDescriptor
                };

                pExtendedFeatures = &featuresRobustness2;
            }

            var featuresExtendedDynamicState = new PhysicalDeviceExtendedDynamicStateFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceExtendedDynamicStateFeaturesExt,
                PNext = pExtendedFeatures,
                ExtendedDynamicState = physicalDevice.IsDeviceExtensionPresent(ExtExtendedDynamicState.ExtensionName)
            };

            pExtendedFeatures = &featuresExtendedDynamicState;

            var featuresVk11 = new PhysicalDeviceVulkan11Features()
            {
                SType = StructureType.PhysicalDeviceVulkan11Features,
                PNext = pExtendedFeatures,
                ShaderDrawParameters = supportedFeaturesVk11.ShaderDrawParameters
            };

            pExtendedFeatures = &featuresVk11;

            var featuresVk12 = new PhysicalDeviceVulkan12Features()
            {
                SType = StructureType.PhysicalDeviceVulkan12Features,
                PNext = pExtendedFeatures,
                DescriptorIndexing = physicalDevice.IsDeviceExtensionPresent("VK_EXT_descriptor_indexing"),
                DrawIndirectCount = physicalDevice.IsDeviceExtensionPresent(KhrDrawIndirectCount.ExtensionName),
                UniformBufferStandardLayout = physicalDevice.IsDeviceExtensionPresent("VK_KHR_uniform_buffer_standard_layout")
            };

            pExtendedFeatures = &featuresVk12;

            PhysicalDeviceIndexTypeUint8FeaturesEXT featuresIndexU8;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_index_type_uint8"))
            {
                featuresIndexU8 = new PhysicalDeviceIndexTypeUint8FeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceIndexTypeUint8FeaturesExt,
                    PNext = pExtendedFeatures,
                    IndexTypeUint8 = true
                };

                pExtendedFeatures = &featuresIndexU8;
            }

            PhysicalDeviceFragmentShaderInterlockFeaturesEXT featuresFragmentShaderInterlock;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_fragment_shader_interlock"))
            {
                featuresFragmentShaderInterlock = new PhysicalDeviceFragmentShaderInterlockFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceFragmentShaderInterlockFeaturesExt,
                    PNext = pExtendedFeatures,
                    FragmentShaderPixelInterlock = true
                };

                pExtendedFeatures = &featuresFragmentShaderInterlock;
            }

            PhysicalDeviceSubgroupSizeControlFeaturesEXT featuresSubgroupSizeControl;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_subgroup_size_control"))
            {
                featuresSubgroupSizeControl = new PhysicalDeviceSubgroupSizeControlFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceSubgroupSizeControlFeaturesExt,
                    PNext = pExtendedFeatures,
                    SubgroupSizeControl = true
                };

                pExtendedFeatures = &featuresSubgroupSizeControl;
            }

            PhysicalDeviceCustomBorderColorFeaturesEXT featuresCustomBorderColor;

            if (physicalDevice.IsDeviceExtensionPresent("VK_EXT_custom_border_color") &&
                supportedFeaturesCustomBorderColor.CustomBorderColors &&
                supportedFeaturesCustomBorderColor.CustomBorderColorWithoutFormat)
            {
                featuresCustomBorderColor = new PhysicalDeviceCustomBorderColorFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceCustomBorderColorFeaturesExt,
                    PNext = pExtendedFeatures,
                    CustomBorderColors = true,
                    CustomBorderColorWithoutFormat = true,
                };

                pExtendedFeatures = &featuresCustomBorderColor;
            }

            var enabledExtensions = _requiredExtensions.Union(_desirableExtensions.Intersect(physicalDevice.DeviceExtensions)).ToArray();

            IntPtr* ppEnabledExtensions = stackalloc IntPtr[enabledExtensions.Length];

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                ppEnabledExtensions[i] = Marshal.StringToHGlobalAnsi(enabledExtensions[i]);
            }

            var deviceCreateInfo = new DeviceCreateInfo()
            {
                SType = StructureType.DeviceCreateInfo,
                PNext = pExtendedFeatures,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,
                PpEnabledExtensionNames = (byte**)ppEnabledExtensions,
                EnabledExtensionCount = (uint)enabledExtensions.Length,
                PEnabledFeatures = &features
            };

            api.CreateDevice(physicalDevice.PhysicalDevice, in deviceCreateInfo, null, out var device).ThrowOnError();

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                Marshal.FreeHGlobal(ppEnabledExtensions[i]);
            }

            return device;
        }
    }
}
