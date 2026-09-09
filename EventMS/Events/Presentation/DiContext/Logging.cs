using Serilog;
using Serilog.Formatting.Compact;

namespace EventMS.Events.Presentation.DiContext.Logging;

public static class SerilogConfiguration
{
    /// <summary>
    /// Настраивает Serilog для приложения.
    /// </summary>
    public static WebApplicationBuilder AddCustomSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .WriteTo.Console(new CompactJsonFormatter())
        );

        return builder;
    }
}