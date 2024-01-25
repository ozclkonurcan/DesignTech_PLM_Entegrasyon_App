using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace DesignTech_PLM_Entegrasyon_App.Application.Interfaces.WindowsFormSettingsInterfaces
{
    public interface IWindowsFormSettingsRepository<T> where T : class
    {
        List<T> GetWindowsFormSettingsListWithBrands();
    }
}
