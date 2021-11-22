using ARMeilleure.Translation.PTC;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using LibHac;
using LibHac.Fs;
using LibHac.FsSystem.NcaUtils;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration;
using Ryujinx.HLE;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderCacheLoadingState = Ryujinx.Graphics.Gpu.Shader.ShaderCacheState;

namespace Ryujinx.Ava.Ui.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        private readonly MainWindow _owner;
        private ObservableCollection<ApplicationData> _applications;
        private DataGridCollectionView _appsCollection;
        private string _aspectStatusText;
        private string _loadHeading;
        private string _cacheLoadStatus;
        private string _searchText;
        private string _dockedStatusText;
        private string _fifoStatusText;
        private string _gameStatusText;
        private string _gpuStatusText;
        private ViewMode _viewMode = Controls.ViewMode.Grid;
        private bool _isAmiiboRequested;
        private bool _isGameRunning;
        private bool _isLoading;
        private string _lastScannedAmiiboId;
        private int _progressMaximum;
        private int _progressValue;
        private int _gridSizeScale = 2;
        private bool _showAll;
        private bool _showLoadProgress;
        private bool _showMenuAndStatusBar = true;
        private bool _showStatusSeparator;
        private Brush _progressBarForegroundColor;
        private Brush _progressBarBackgroundColor;
        private Brush _vsyncColor;
        private byte[] _selectedIcon;
        private bool _isAppletMenuActive;
        private int _statusBarProgressMaximum;
        private int _statusBarProgressValue;
        private bool _isPaused;
        private bool _showNames;
        private bool _showContent = true;
        private bool _isLoadingIndeterminate = true;
        private ReadOnlyObservableCollection<ApplicationData> _appsObservableList;
        private ApplicationSort _sortMode = ApplicationSort.Favorite;

        public MainWindowViewModel(MainWindow owner) : this()
        {
            _owner = owner;
        }

        public MainWindowViewModel()
        {
            Applications = new ObservableCollection<ApplicationData>();
            
            Applications.ToObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out _appsObservableList).AsObservableList();
            AppsCollection = new DataGridCollectionView(Applications)
            {
                Filter = Filter
            };

            AppsCollection.SortDescriptions.Add(DataGridSortDescription.FromPath("Favorite", System.ComponentModel.ListSortDirection.Descending));

            ApplicationLibrary.ApplicationCountUpdated += ApplicationLibrary_ApplicationCountUpdated;
            ApplicationLibrary.ApplicationAdded += ApplicationLibrary_ApplicationAdded;

            Ptc.PtcStateChanged -= ProgressHandler;
            Ptc.PtcStateChanged += ProgressHandler;

            if (Program.PreviewerDetached)
            {
                ShowUiKey     = KeyGesture.Parse(ConfigurationState.Instance.Hid.Hotkeys.Value.ShowUi.ToString());
                ScreenshotKey = KeyGesture.Parse(ConfigurationState.Instance.Hid.Hotkeys.Value.Screenshot.ToString());
                PauseKey      = KeyGesture.Parse(ConfigurationState.Instance.Hid.Hotkeys.Value.Pause.ToString());
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                RefreshView();
            }
        }

        public DataGridCollectionView AppsCollection
        {
            get => _appsCollection;
            set
            {
                _appsCollection = value;

                OnPropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<ApplicationData> AppsObservableList
        {
            get => _appsObservableList;
            set
            {
                _appsObservableList = value;

                OnPropertyChanged();
            }
        }

        public bool ShowFavoriteColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.FavColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.FavColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowIconColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.IconColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.IconColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;

                OnPropertyChanged();
            }
        }

        public bool EnableNonGameRunningControls => !IsGameRunning;

        public bool ShowFirmwareStatus => !ShowLoadProgress;

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set
            {
                _isGameRunning = value;

                if (!value)
                {
                    ShowMenuAndStatusBar = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableNonGameRunningControls));
                OnPropertyChanged(nameof(ShowFirmwareStatus));
            }
        }

        public bool IsAmiiboRequested
        {
            get => _isAmiiboRequested && _isGameRunning;
            set
            {
                _isAmiiboRequested = value;

                OnPropertyChanged();
            }
        }

        public bool ShowLoadProgress
        {
            get => _showLoadProgress;
            set
            {
                _showLoadProgress = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowFirmwareStatus));
            }
        }

        public string GameStatusText
        {
            get => _gameStatusText;
            set
            {
                _gameStatusText = value;

                OnPropertyChanged();
            }
        }

        private string _showUikey     = "F4";
        private string _pauseKey      = "F5";
        private string _screenshotkey = "F8";

        public ApplicationData SelectedApplication
        {
            get
            {
                switch (ViewMode)
                {
                    case ViewMode.List:
                        return _owner.GameList.SelectedItem as ApplicationData;
                        break;
                    case ViewMode.Grid:
                        return _owner.GameGrid.SelectedApplication;
                        break;
                    default:
                        return null;
                }
            }
        }

        public string LoadHeading
        {
            get => _loadHeading;
            set
            {
                _loadHeading = value;

                OnPropertyChanged();
            }
        }

        public string CacheLoadStatus
        {
            get => _cacheLoadStatus;
            set
            {
                _cacheLoadStatus = value;

                OnPropertyChanged();
            }
        }

        public Brush ProgressBarBackgroundColor
        {
            get => _progressBarBackgroundColor;
            set
            {
                _progressBarBackgroundColor = value;

                OnPropertyChanged();
            }
        }

        public Brush ProgressBarForegroundColor
        {
            get => _progressBarForegroundColor;
            set
            {
                _progressBarForegroundColor = value;

                OnPropertyChanged();
            }
        }

        public Brush VsyncColor
        {
            get => _vsyncColor;
            set
            {
                _vsyncColor = value;

                OnPropertyChanged();
            }
        }

        public byte[] SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;

                OnPropertyChanged();
            }
        }

        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                _progressMaximum = value;

                OnPropertyChanged();
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;

                OnPropertyChanged();
            }
        }

        public int StatusBarProgressMaximum
        {
            get => _statusBarProgressMaximum;
            set
            {
                _statusBarProgressMaximum = value;

                OnPropertyChanged();
            }
        }

        public int StatusBarProgressValue
        {
            get => _statusBarProgressValue;
            set
            {
                _statusBarProgressValue = value;

                OnPropertyChanged();
            }
        }

        public string FifoStatusText
        {
            get => _fifoStatusText;
            set
            {
                _fifoStatusText = value;

                OnPropertyChanged();
            }
        }

        public string GpuStatusText
        {
            get => _gpuStatusText;
            set
            {
                _gpuStatusText = value;

                OnPropertyChanged();
            }
        }

        public string DockedStatusText
        {
            get => _dockedStatusText;
            set
            {
                _dockedStatusText = value;

                OnPropertyChanged();
            }
        }

        public string AspectRatioStatusText
        {
            get => _aspectStatusText;
            set
            {
                _aspectStatusText = value;

                OnPropertyChanged();
            }
        }

        public bool ShowStatusSeparator
        {
            get => _showStatusSeparator;
            set
            {
                _showStatusSeparator = value;

                OnPropertyChanged();
            }
        }

        public Thickness GridItemPadding => ShowNames ? new Thickness() : new Thickness(5);

        public bool ShowMenuAndStatusBar
        {
            get => _showMenuAndStatusBar;
            set
            {
                _showMenuAndStatusBar = value;

                OnPropertyChanged();
            }
        }
        
        public bool IsLoadingIndeterminate
        {
            get => _isLoadingIndeterminate;
            set
            {
                _isLoadingIndeterminate = value;

                OnPropertyChanged();
            }
        }

        public bool ShowContent
        {
            get => _showContent;
            set
            {
                _showContent = value;

                OnPropertyChanged();
            }
        }
        
        public bool IsAppletMenuActive
        {
            get => _isAppletMenuActive && EnableNonGameRunningControls;
            set
            {
                _isAppletMenuActive = value;

                OnPropertyChanged();
            }
        }

        public bool IsGrid => ViewMode == ViewMode.Grid;
        public bool IsList => ViewMode == ViewMode.List;

        public bool ShowTitleColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.AppColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.AppColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        internal void Sort(bool isAscending)
        {
            IsAscending = isAscending;
            RefreshView();
        }

        internal void Sort(ApplicationSort sort)
        {
            SortMode = sort;
            RefreshView();
        }

        private IComparer<ApplicationData> GetComparer()
        {
            switch (SortMode)
            {
                case ApplicationSort.LastPlayed:
                    return new Models.Generic.LastPlayedSortComparer(IsAscending);
                case ApplicationSort.FileSize:
                    return new Models.Generic.FileSizeSortComparer(IsAscending);
                case ApplicationSort.TotalTimePlayed:
                    return new Models.Generic.TimePlayedSortComparer(IsAscending);
                case ApplicationSort.Title:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.ApplicationName) : SortExpressionComparer<ApplicationData>.Descending(app => app.ApplicationName);
                case ApplicationSort.Favorite:
                    return !IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Favorite) : SortExpressionComparer<ApplicationData>.Descending(app => app.Favorite);
                case ApplicationSort.Developer:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Developer) : SortExpressionComparer<ApplicationData>.Descending(app => app.Developer);
                case ApplicationSort.FileType:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.FileExtension) : SortExpressionComparer<ApplicationData>.Descending(app => app.FileExtension);
                case ApplicationSort.Path:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Path) : SortExpressionComparer<ApplicationData>.Descending(app => app.Path);
                default:
                    return null;
            }
        }

        private void RefreshView()
        {
            AppsCollection.Refresh();
            
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            Applications.ToObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out _appsObservableList).AsObservableList();

            OnPropertyChanged(nameof(AppsObservableList));
        }

        public bool ShowDeveloperColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.DevColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.DevColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowVersionColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.VersionColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.VersionColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowTimePlayedColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowLastPlayedColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowFileExtColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowFileSizeColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool ShowFilePathColumn
        {
            get => ConfigurationState.Instance.Ui.GuiColumns.PathColumn;
            set
            {
                ConfigurationState.Instance.Ui.GuiColumns.PathColumn.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                _owner.UpdateGridColumns();

                OnPropertyChanged();
            }
        }

        public bool StartGamesInFullscreen
        {
            get => ConfigurationState.Instance.Ui.StartFullscreen;
            set
            {
                ConfigurationState.Instance.Ui.StartFullscreen.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                OnPropertyChanged();
            }
        }

        public ObservableCollection<ApplicationData> Applications
        {
            get => _applications;
            set
            {
                _applications = value;

                OnPropertyChanged();
            }
        }

        public ViewMode ViewMode
        {
            get => _viewMode; set
            {
                _viewMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGrid));
                OnPropertyChanged(nameof(IsList));
            }
        }

        public bool ShowNames
        {
            get => _showNames && _gridSizeScale > 1; set
            {
                _showNames = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GridItemPadding));
            }
        }

        public ApplicationSort SortMode
        {
            get => _sortMode; private set
            {
                _sortMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SortName));
            }
        }

        public string SortName
        {
            get
            {
                switch (SortMode)
                {
                    case ApplicationSort.Title:
                        return LocaleManager.Instance["GameListHeaderApplication"];
                    case ApplicationSort.Developer:
                        return LocaleManager.Instance["GameListHeaderDeveloper"];
                    case ApplicationSort.LastPlayed:
                        return LocaleManager.Instance["GameListHeaderLastPlayed"];
                    case ApplicationSort.TotalTimePlayed:
                        return LocaleManager.Instance["GameListHeaderTimePlayed"];
                    case ApplicationSort.FileType:
                        return LocaleManager.Instance["GameListHeaderFileExtension"];
                    case ApplicationSort.FileSize:
                        return LocaleManager.Instance["GameListHeaderFileSize"];
                    case ApplicationSort.Path:
                        return LocaleManager.Instance["GameListHeaderPath"];
                    case ApplicationSort.Favorite:
                        return LocaleManager.Instance["CommonFavorite"];
                }

                return string.Empty;
            }
        }

        public bool IsAscending { get; private set; } = true;

        public KeyGesture ShowUiKey
        {
            get => KeyGesture.Parse(_showUikey); set
            {
                _showUikey = value.ToString();
                OnPropertyChanged();
            }
        }

        public KeyGesture ScreenshotKey
        {
            get => KeyGesture.Parse(_screenshotkey); set
            {
                _screenshotkey = value.ToString();
                OnPropertyChanged();
            }
        }

        public KeyGesture PauseKey
        {
            get => KeyGesture.Parse(_pauseKey); set
            {
                _pauseKey = value.ToString();
                OnPropertyChanged();
            }
        }

        public bool IsGridSmall => _gridSizeScale == 1;
        public bool IsGridNormal => _gridSizeScale == 2;
        public bool IsGridLarge => _gridSizeScale == 3;
        public bool IsGridHuge => _gridSizeScale == 4;

        public int GridSizeScale
        {
            get => _gridSizeScale;
            set
            {
                _gridSizeScale = value;
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGridSmall));
                OnPropertyChanged(nameof(IsGridNormal));
                OnPropertyChanged(nameof(IsGridLarge));
                OnPropertyChanged(nameof(IsGridHuge));
                OnPropertyChanged(nameof(ShowNames));
            }
        }

        public async void OpenAmiiboWindow()
        {
            if (!_isAmiiboRequested)
            {
                return;
            }

            if (_owner.AppHost.Device.System.SearchingForAmiibo(out int deviceId))
            {
                string titleId = _owner.AppHost.Device.Application.TitleIdText.ToUpper();
                AmiiboWindow window = new(_showAll, _lastScannedAmiiboId, titleId);

                await window.ShowDialog(_owner);

                if (window.IsScanned)
                {
                    _showAll = window.ViewModel.ShowAllAmiibo;
                    _lastScannedAmiiboId = window.ScannedAmiibo.GetId();

                    _owner.AppHost.Device.System.ScanAmiibo(deviceId, _lastScannedAmiiboId, window.ViewModel.UseRandomUuid);
                }
            }
        }

        public void HandleShaderProgress(Switch emulationContext)
        {
            emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
            emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
        }

        private bool Filter(object arg)
        {
            if (arg is ApplicationData app)
            {
                return string.IsNullOrWhiteSpace(_searchText) || app.ApplicationName.ToLower().Contains(_searchText.ToLower());
            }

            return false;
        }

        private void ApplicationLibrary_ApplicationAdded(object sender, ApplicationAddedEventArgs e)
        {
            AddApplication(e.AppData);
            
            //RefreshGrid();
        }

        private void ApplicationLibrary_ApplicationCountUpdated(object sender, ApplicationCountUpdatedEventArgs e)
        {
            StatusBarProgressValue = e.NumAppsLoaded;
            StatusBarProgressMaximum = e.NumAppsFound;
            LocaleManager.Instance.UpdateDynamicValue("StatusBarGamesLoaded", StatusBarProgressValue, StatusBarProgressMaximum);

            Dispatcher.UIThread.Post(() =>
            {
                if (e.NumAppsFound == 0)
                {
                    _owner.LoadProgressBar.IsVisible = false;
                }
            });
        }

        public void AddApplication(ApplicationData applicationData)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Applications.Add(applicationData);
            });
        }

        public async void LoadApplications()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Applications.Clear();
                _owner.LoadProgressBar.IsVisible = true;
                StatusBarProgressMaximum = 0;
                StatusBarProgressValue = 0;
                LocaleManager.Instance.UpdateDynamicValue("StatusBarGamesLoaded", 0, 0);
            });

            ReloadGameList();
        }

        private void ReloadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            Thread thread = new(() =>
            {
                ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs, _owner.VirtualFileSystem, ConfigurationState.Instance.System.Language);

                _isLoading = false;
            })
            { Name = "GUI.AppListLoadThread", Priority = ThreadPriority.AboveNormal };

            thread.Start();
        }

        public async void OpenFile()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Select a supported file to open"
            };
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = "All Supported Formats",
                Extensions =
                {
                    "nsp",
                    "pfs0",
                    "xci",
                    "nca",
                    "nro",
                    "nso"
                }
            });
            dialog.Filters.Add(new FileDialogFilter { Name = "NSP", Extensions = { "nsp" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "PFS0", Extensions = { "pfs0" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "XCI", Extensions = { "xci" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NCA", Extensions = { "nca" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NRO", Extensions = { "nro" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NSO", Extensions = { "nso" } });

            string[] files = await dialog.ShowAsync(_owner);

            if (files != null && files.Length > 0)
            {
                _owner.LoadApplication(files[0]);
            }
        }

        public async void OpenFolder()
        {
            OpenFolderDialog dialog = new()
            {
                Title = "Select a folder with an unpacked game"
            }; ;

            string folder = await dialog.ShowAsync(_owner);

            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                _owner.LoadApplication(folder);
            }
        }

        public async void TakeScreenshot()
        {
            _owner.AppHost.ScreenshotRequested = true;
        }

        public async void HideUi()
        {
            ShowMenuAndStatusBar = false;
        }

        public async void SetListMode()
        {
            ViewMode = ViewMode.List;
        }

        public async void SetGridMode()
        {
            ViewMode = ViewMode.Grid;
        }

        public async void OpenMiiApplet()
        {
            string contentPath = _owner.ContentManager.GetInstalledContentPath(0x0100000000001009, StorageId.NandSystem, NcaContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                _owner.LoadApplication(contentPath);
            }
        }

        public void OpenRyujinxFolder()
        {
            OpenHelper.OpenFolder(AppDataManager.BaseDirPath);
        }

        public void OpenLogsFolder()
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            new DirectoryInfo(logPath).Create();

            OpenHelper.OpenFolder(logPath);
        }

        public void ToggleFullscreen()
        {
            WindowState state = _owner.WindowState;

            if (state == WindowState.FullScreen)
            {
                _owner.WindowState = WindowState.Normal;
            }
            else
            {
                _owner.WindowState = WindowState.FullScreen;

                if (IsGameRunning)
                {
                    ShowMenuAndStatusBar = false;
                }
            }
        }

        public async void OpenSettings()
        {
            _owner.SettingsWindow = new(_owner.VirtualFileSystem, _owner.ContentManager);

            await _owner.SettingsWindow.ShowDialog(_owner);
        }

        public async void ManageProfiles()
        {
            UserProfileWindow window = new(_owner.AccountManager, _owner.ContentManager, _owner.VirtualFileSystem);

            await window.ShowDialog(_owner);
        }

        public async void OpenAboutWindow()
        {
            AboutWindow window = new();

            await window.ShowDialog(_owner);
        }

        public void ChangeLanguage(object obj)
        {
            LocaleManager.Instance.LoadLanguage((string)obj);
        }

        private void ProgressHandler<T>(T state, int current, int total) where T : Enum
        {
            try
            {
                ProgressMaximum = total;
                ProgressValue = current;

                switch (state)
                {
                    case PtcLoadingState ptcState:
                        CacheLoadStatus = $"{current} / {total}";
                        switch (ptcState)
                        {
                            case PtcLoadingState.Start:
                            case PtcLoadingState.Loading:
                                LoadHeading = "Compiling PTC";
                                IsLoadingIndeterminate = false;
                                break;
                            case PtcLoadingState.Loaded:
                                LoadHeading = $"Loading {SelectedApplication.TitleName}";
                                IsLoadingIndeterminate = true;
                                CacheLoadStatus = "";
                                break;
                        }
                        break;
                    case ShaderCacheLoadingState shaderCacheState:
                        CacheLoadStatus = $"{current} / {total}";
                        switch (shaderCacheState)
                        {
                            case ShaderCacheLoadingState.Start:
                            case ShaderCacheLoadingState.Loading:
                                LoadHeading = "Compiling shaders";
                                IsLoadingIndeterminate = false;
                                break;
                            case ShaderCacheLoadingState.Loaded:
                                LoadHeading = $"Loading {SelectedApplication.TitleName}";
                                IsLoadingIndeterminate = true;
                                CacheLoadStatus = "";
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}");
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public void OpenUserSaveDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                SaveDataFilter filter = new();
                filter.SetUserId(new UserId(1, 0));
                OpenSaveDirectory(filter, selection);
            }
        }

        public void ToggleFavorite()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                selection.Favorite = !selection.Favorite;

                RefreshView();
            }
        }

        public void OpenModsDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                string modsBasePath = _owner.VirtualFileSystem.ModLoader.GetModsBasePath();
                string titleModsPath = _owner.VirtualFileSystem.ModLoader.GetTitleDir(modsBasePath, selection.TitleId);

                OpenHelper.OpenFolder(titleModsPath);
            }
        }

        public void OpenPtcDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                string ptcDir = Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu");

                string mainPath = Path.Combine(ptcDir, "0");
                string backupPath = Path.Combine(ptcDir, "1");

                if (!Directory.Exists(ptcDir))
                {
                    Directory.CreateDirectory(ptcDir);
                    Directory.CreateDirectory(mainPath);
                    Directory.CreateDirectory(backupPath);
                }

                OpenHelper.OpenFolder(ptcDir);
            }
        }

        public async void PurgePtcCache()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                DirectoryInfo mainDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "0"));
                DirectoryInfo backupDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "1"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(_owner, LocaleManager.Instance["DialogWarning"],
                    string.Format(LocaleManager.Instance["DialogPPTCDeletionMessage"], selection.TitleName));

                List<FileInfo> cacheFiles = new();

                if (mainDir.Exists)
                {
                    cacheFiles.AddRange(mainDir.EnumerateFiles("*.cache"));
                }

                if (backupDir.Exists)
                {
                    cacheFiles.AddRange(backupDir.EnumerateFiles("*.cache"));
                }

                if (cacheFiles.Count > 0 && result == UserResult.Yes)
                {
                    foreach (FileInfo file in cacheFiles)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            ContentDialogHelper.CreateErrorDialog(_owner, string.Format(LocaleManager.Instance["DialogPPTCDeletionErrorMessage"], file.Name, e));
                        }
                    }
                }
            }
        }

        public void OpenShaderCacheDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                string shaderCacheDir = Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "shader");

                if (!Directory.Exists(shaderCacheDir))
                {
                    Directory.CreateDirectory(shaderCacheDir);
                }

                OpenHelper.OpenFolder(shaderCacheDir);
            }
        }

        public void SimulateWakeUpMessage()
        {
            _owner.AppHost.Device.System.SimulateWakeUpMessage();
        }

        public async void PurgeShaderCache()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                DirectoryInfo shaderCacheDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "shader"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(_owner, LocaleManager.Instance["DialogWarning"],
                    string.Format(LocaleManager.Instance["DialogShaderDeletionMessage"], selection.TitleName));

                List<DirectoryInfo> cacheDirectory = new();

                if (shaderCacheDir.Exists)
                {
                    cacheDirectory.AddRange(shaderCacheDir.EnumerateDirectories("*"));
                }

                if (cacheDirectory.Count > 0 && result == UserResult.Yes)
                {
                    foreach (DirectoryInfo directory in cacheDirectory)
                    {
                        try
                        {
                            directory.Delete(true);
                        }
                        catch (Exception e)
                        {
                            ContentDialogHelper.CreateErrorDialog(_owner,
                                string.Format(LocaleManager.Instance["DialogPPTCDeletionErrorMessage"], directory.Name,
                                    e));
                        }
                    }
                }
            }
        }

        public async void CheckForUpdates()
        {
            if (Updater.CanUpdate(true, _owner))
            {
                await Updater.BeginParse(_owner, true);
            }
        }

        public async void OpenTitleUpdateManager()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                TitleUpdateWindow titleUpdateManager = new(_owner.VirtualFileSystem, selection.TitleId, selection.TitleName);

                await titleUpdateManager.ShowDialog(_owner);
            }
        }

        public async void OpenDlcManager()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                DlcManagerWindow dlcManager = new(_owner.VirtualFileSystem, selection.TitleId, selection.TitleName);

                await dlcManager.ShowDialog(_owner);
            }
        }


        public void OpenDeviceSaveDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                SaveDataFilter filter = new();
                filter.SetSaveDataType(SaveDataType.Device);
                OpenSaveDirectory(filter, selection);
            }
        }

        public void OpenBcatSaveDirectory()
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                SaveDataFilter filter = new();
                filter.SetSaveDataType(SaveDataType.Bcat);
                OpenSaveDirectory(filter, selection);
            }
        }

        private void OpenSaveDirectory(SaveDataFilter filter, ApplicationData data)
        {
            if (!ulong.TryParse(data.TitleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                out ulong titleIdNumber))
            {
                ContentDialogHelper.CreateErrorDialog(_owner,
                    LocaleManager.Instance["DialogRyujinxErrorMessage"], LocaleManager.Instance["DialogInvalidTitleIdErrorMessage"]);

                return;
            }

            Task.Run(() => ApplicationHelper.OpenSaveDir(filter, titleIdNumber, data.ControlHolder, data.TitleName));
        }

        private void ExtractLogo()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                ApplicationHelper.ExtractSection(NcaSectionType.Logo, selection.Path);
            }
        }

        private void ExtractRomFs()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                ApplicationHelper.ExtractSection(NcaSectionType.Data, selection.Path);
            }
        }

        private void ExtractExeFs()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                ApplicationHelper.ExtractSection(NcaSectionType.Code, selection.Path);
            }
        }

        public void CloseWindow()
        {
            _owner.Close();
        }

        private async void HandleFirmwareInstallation(string path)
        {
            try
            {
                string filename = path;

                SystemVersion firmwareVersion = _owner.ContentManager.VerifyFirmwarePackage(filename);

                if (firmwareVersion == null)
                {
                    ContentDialogHelper.CreateErrorDialog(_owner, string.Format(LocaleManager.Instance["DialogFirmwareInstallerFirmwareNotFoundErrorMessage"], filename));

                    return;
                }

                string dialogTitle =
                    string.Format(LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallTitle"],
                        firmwareVersion.VersionString);


                SystemVersion currentVersion = _owner.ContentManager.GetCurrentFirmwareVersion();

                string dialogMessage = string.Format(LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallMessage"],
                    firmwareVersion.VersionString);

                if (currentVersion != null)
                {
                    dialogMessage +=
                        string.Format(LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallSubMessage"],
                            currentVersion.VersionString);
                }

                dialogMessage += LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallConfirmMessage"];

                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(_owner, dialogTitle, dialogMessage);

                UpdateWaitWindow waitingDialog = ContentDialogHelper.CreateWaitingDialog(dialogTitle, LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallWaitMessage"]);

                if (result == UserResult.Yes)
                {
                    Logger.Info?.Print(LogClass.Application, $"Installing firmware {firmwareVersion.VersionString}");

                    Thread thread = new(() =>
                    {
                        Dispatcher.UIThread.InvokeAsync(delegate
                        {
                            waitingDialog.Show();
                        });

                        try
                        {
                            _owner.ContentManager.InstallFirmware(filename);

                            Dispatcher.UIThread.InvokeAsync(async delegate
                            {
                                waitingDialog.Close();

                                string message = string.Format(
                                    LocaleManager.Instance["DialogFirmwareInstallerFirmwareInstallSuccessMessage"],
                                    firmwareVersion.VersionString);

                                await ContentDialogHelper.CreateInfoDialog(_owner, dialogTitle, message, "", LocaleManager.Instance["InputDialogOk"]);
                                Logger.Info?.Print(LogClass.Application, message);

                                // Purge Applet Cache.

                                DirectoryInfo miiEditorCacheFolder = new DirectoryInfo(System.IO.Path.Combine(AppDataManager.GamesDirPath, "0100000000001009", "cache"));

                                if (miiEditorCacheFolder.Exists)
                                {
                                    miiEditorCacheFolder.Delete(true);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.UIThread.InvokeAsync(async delegate
                            {
                                waitingDialog.Close();

                                ContentDialogHelper.CreateErrorDialog(_owner, ex.Message);
                            });
                        }
                        finally
                        {
                            _owner.RefreshFirmwareStatus();
                        }
                    });

                    thread.Name = "GUI.FirmwareInstallerThread";
                    thread.Start();
                }
            }
            catch (LibHac.MissingKeyException ex)
            {
                Logger.Error?.Print(LogClass.Application, ex.ToString());
                UserErrorDialog.ShowUserErrorDialog(UserError.NoKeys, _owner);
            }
            catch (Exception ex)
            {
                ContentDialogHelper.CreateErrorDialog(_owner, ex.Message);
            }
        }

        public async void InstallFirmwareFromFile()
        {
            OpenFileDialog dialog = new() { AllowMultiple = false };
            dialog.Filters.Add(new FileDialogFilter { Name = "All types", Extensions = { "xci", "zip" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "XCI", Extensions = { "xci" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "ZIP", Extensions = { "zip" } });

            string[] file = await dialog.ShowAsync(_owner);

            if (file != null && file.Length > 0)
            {
                HandleFirmwareInstallation(file[0]);
            }
        }

        public async void InstallFirmwareFromFolder()
        {
            OpenFolderDialog dialog = new();

            string folder = await dialog.ShowAsync(_owner);

            if (!string.IsNullOrWhiteSpace(folder))
            {
                HandleFirmwareInstallation(folder);
            }
        }
    }
}