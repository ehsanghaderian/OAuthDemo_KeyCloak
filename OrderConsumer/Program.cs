using OrderConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IOAuthTokenService, OAuthTokenService>(option=>
{
    option.BaseAddress = new Uri("http://keycloak:8080/");
});

builder.Services.AddHttpClient<IOrderService ,OrderService>(option =>
{
    option.BaseAddress = new Uri("http://oauthdemo:8080/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();


app.MapControllers();

app.Run();
