using System.ComponentModel;
using System.Configuration.Install;

namespace Samples.AWriteModel
{
    [RunInstaller(true)]
    public partial class WinServiceInstaller : Installer
    {
        public WinServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
