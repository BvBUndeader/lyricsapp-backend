using LyricsBackend.Contracts;
using LyricsBackend.Models;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(provider =>
{
    var url = builder.Configuration["Supabase:SupabaseUrl"];
    var key = builder.Configuration["Supabase:SupabaseKey"];
    var options = new SupabaseOptions { AutoConnectRealtime = false };
    var client = new Supabase.Client(url, key, options);
    client.InitializeAsync().Wait();
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/users", async (
    CreateUserRequest request, Supabase.Client client) =>
{
    var user = new Users
    {
        Username = request.Username,
        Password = request.Password,
        Email = request.Email
    };

    var response = await client.From<Users>().Insert(user);

    var newUser = response.Models.First();

    return Results.Ok(newUser.Id);
});

app.MapGet("/users/{id}", async (long id, Supabase.Client client) =>
{
    var response = await client.
    From<Users>().Where(n => n.Id == id).Get();

    var user = response.Models.FirstOrDefault();

    if (user is null)
    {
        return Results.NotFound();
    }

    var userResponse = new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Password = user.Password,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };

    return Results.Ok(userResponse);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
