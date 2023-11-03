namespace DesignTech_PLM_Entegrasyon_App.MVC.Models;

using Serilog.Events;

public static class CustomLogEventLevelExtensions
{


    public static LogEventLevel ToSerilogLevel(this CustomLogEventLevel customLevel)
    {

        switch (customLevel)
        {
            case CustomLogEventLevel.CustomInformation:
                return LogEventLevel.Information;
            case CustomLogEventLevel.CustomWarning:
                return LogEventLevel.Warning;
            case CustomLogEventLevel.CustomError:
                return LogEventLevel.Error;
            case CustomLogEventLevel.CustomDebug:
                return LogEventLevel.Debug;
            default:
                return LogEventLevel.Information; // Varsayılan seviye
        }
    }
}

