using Ryujinx.Ava.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Ava.Ui.Models.Generic
{
    public class LastPlayedSortComparer : IComparer<ApplicationData>
    {
        public LastPlayedSortComparer() { }
        public LastPlayedSortComparer(bool isAscending) { IsAscending = isAscending; }

        public bool IsAscending { get; }

        public int Compare(ApplicationData? x, ApplicationData? y)
        {
            string aValue = x.LastPlayed;
            string bValue = y.LastPlayed;

            if (aValue == "Never")
            {
                aValue = DateTime.UnixEpoch.ToString();
            }

            if (bValue == "Never")
            {
                bValue = DateTime.UnixEpoch.ToString();
            }

            return (IsAscending ? 1 : -1) * DateTime.Compare(DateTime.Parse(bValue), DateTime.Parse(aValue));
        }
    }
}