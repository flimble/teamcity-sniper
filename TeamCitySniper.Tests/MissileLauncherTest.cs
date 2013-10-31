using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using MissileSharp;
using Moq;
using NUnit.Framework;

namespace TeamCitySniper.Tests
{
    [TestFixture]
    public class MissileLauncherTest
    {
        [Test]
        public void execute_with_matching_cmdsetname_found_loadscommandtodreamcheeky_and_resets()
        {
            string cmdsetName = "david.jason";

            var mockCommandCentre = new Mock<ICommandCenter>();
            mockCommandCentre.Setup(x => x.GetLoadedCommandSetNames()).Returns(new List<string> {cmdsetName});

            var underTest = new MissileLauncher(mockCommandCentre.Object, new string[] {});

            underTest.Execute(cmdsetName);

            mockCommandCentre.Verify(x => x.RunCommandSet(cmdsetName), Times.Once);
            mockCommandCentre.Verify(x => x.Reset(), Times.Once);
        }

        [Test]
        public void execute_with_no_user_found_does_nothing()
        {
            string cmdsetName = "david.jason";

            var mockCommandCentre = new Mock<ICommandCenter>();
            mockCommandCentre.Setup(x => x.GetLoadedCommandSetNames()).Returns(new List<string>());

            var underTest = new MissileLauncher(mockCommandCentre.Object, new string[] {});
            underTest.Execute(cmdsetName);

            mockCommandCentre.Verify(x => x.RunCommandSet(cmdsetName), Times.Never);
            mockCommandCentre.Verify(x => x.Reset(), Times.Never);
        }

      



    }
}
