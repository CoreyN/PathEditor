using System.Security.Principal;
using System.Windows;

namespace PathEditor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!HaveAdminRights())
            {
                MessageBox.Show("You must have administrator rights to make modifications.");
            }
        }

        private bool HaveAdminRights()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
