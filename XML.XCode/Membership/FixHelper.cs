using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core;

namespace XML.XCode.Membership;

internal static class FixHelper
{
    public static String TrimName(this String name, params String[] ss)
    {
        foreach (var item in ss)
        {
            if (name.Length <= 2) break;

            name = name.TrimEnd(item);
        }

        return name;
    }
}