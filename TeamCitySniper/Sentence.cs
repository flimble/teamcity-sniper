using System.Collections.Generic;
using System.Reflection;
using MissileSharp;
using log4net;

namespace TeamCitySniper
{
    public class MissileLauncher : IShootMissiles
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICommandCenter _launcher;        

        public MissileLauncher(ICommandCenter launcher, string suspectLocationsFile)
        {
            _launcher = launcher;            
            _launcher.LoadCommandSets(suspectLocationsFile);
        }

        public IList<string> Suspects
        {
            get { return _launcher.GetLoadedCommandSetNames(); }
        } 

        public void Execute(string perp)
        {
            
            var knownTargets = Suspects;

            if (knownTargets.Contains(perp))
            {
                Logger.InfoFormat("Gaze into the face of Fear, {0}", perp);
                _launcher.RunCommandSet(perp);
                _launcher.Reset();
                Logger.InfoFormat("Death. Court's adjourned.");
            }
            else
            {
                Logger.InfoFormat("Lucky escape for you {0}, next time won't be so easy", perp);
            }
        }
    }
}
