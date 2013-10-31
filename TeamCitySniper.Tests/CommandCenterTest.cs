using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileSharp;
using Moq;
using NUnit.Framework;

namespace TeamCitySniper.Tests
{
    /// <summary>
    /// Tests for 3rd party library MissileSharp that controls the DreamCheeky missile launcher
    /// </summary>
    [TestFixture]
    public class CommandCenterTest
    {
        [Test]
        public void CommandCentre_LoadCommandSets_Correctly_Loads_Config_Content()
        {
            string[] settings = @"
[chris.auret]
reset,0
right,1800
up,400
fire,2

[craig.hicks]
reset,0
right,600
up,300
fire,2

[ravi.mohan]
reset,0
right,150
fire,2

[tarek.lawandi]
reset,0
right,800
up,300
fire,2".Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var underTest = new CommandCenter(Mock.Of<ILauncherModel>());


            underTest.LoadCommandSets(settings);


            Assert.AreEqual(new List<string> { "chris.auret", "craig.hicks", "ravi.mohan", "tarek.lawandi" }, underTest.GetLoadedCommandSetNames());
        }
    }
}
