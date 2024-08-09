using BStockBot.Worker;
using BStockBot.Worker.Models;
using BStockBot.Worker.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddHttpClient();
        services.AddHttpClient("BStock", o =>
        {
            o.DefaultRequestHeaders.TryAddWithoutValidation("Host", "bstock.com");
            o.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9,application/json");
            o.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.62");
            o.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Microsoft Edge\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
            o.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://bstock.com/mobilecarrier/auction/auction/list/?p=1");
            o.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            o.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            o.DefaultRequestHeaders.TryAddWithoutValidation("X-Forwarded-For", builder.Configuration["ConnectingIP"]);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new CustomCookieClientHandler()
        {
            AllowAutoRedirect = false,
            UseCookies = true,
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });

        var bStockOptions = new BStockOptions()
        {
            Username = builder.Configuration["LoginId"],
            Password = builder.Configuration["Password"],
            Endpoint = builder.Configuration["Endpoint"],
            Marketplace = builder.Configuration["Marketplace"],
            ConnectingIP = builder.Configuration["ConnectingIP"]
        };

        services.AddSingleton(bStockOptions);
        services.AddSingleton<BStockService>();

        if (builder.Configuration["Mode"] == "ClosingPrices")
            services.AddHostedService<ClosingPricesWorker>();
        else if (builder.Configuration["Mode"] == "AuctionList")
            services.AddHostedService<AuctionListWorker>();
        else
            throw new ArgumentOutOfRangeException("Mode", "Mode must be of 'ClosingPrices' or 'AuctionList', cannot start daemon.");
    })
    .Build();

host.Run();
