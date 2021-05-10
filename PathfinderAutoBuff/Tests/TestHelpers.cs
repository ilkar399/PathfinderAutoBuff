using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathfinderAutoBuff.Main;

#if (DEBUG)
namespace PathfinderAutoBuff.Tests
{
    class TestHelpers
    {
        public static void TestLog(string testName, string text)
        {
            Logger.Log($"TEST: {testName} - {text}");
        }

        public static void DetailedLog(string text)
        {
            if (PathfinderAutoBuff.Menu.Tests.detailed)
                Logger.Log(text);
        }
    }
}
#endif