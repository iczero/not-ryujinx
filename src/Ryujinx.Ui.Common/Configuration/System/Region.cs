﻿using Ryujinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.Configuration.System
{
    [JsonConverter(typeof(TypedStringEnumConverter<Region>))]
    public enum Region
    {
        Japan,
        USA,
        Europe,
        Australia,
        China,
        Korea,
        Taiwan
    }
}