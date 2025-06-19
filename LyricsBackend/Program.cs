using LyricsBackend.Contracts;
using LyricsBackend.Models;
using Supabase;
using Supabase.Postgrest;
using System.Linq.Expressions;

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

//creatin users
app.MapPost("/users", async (
    CreateUserRequest request, Supabase.Client client) =>
{
    var user = new Users
    {
        Username = request.Username,
        Password = request.Password,
        Email = request.Email,
        CreatedAt = DateTime.UtcNow
    };


    var existingCheck = await client.From<Users>().Where(u => u.Username == user.Username || u.Email == user.Email).Get();
    var foundExistingUser = existingCheck.Models.FirstOrDefault();

    if (foundExistingUser != null)
    {
        if (foundExistingUser.Username == user.Username)
        {
            return Results.Conflict("Username already exists");
        }

        return Results.Conflict("Email already exists");
    }


    var response = await client.From<Users>().Insert(user);

    var newUser = response.Models.First();

    return Results.Created("User created successfully",newUser.Id);
});

// gettin user info
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

//fetching a song
app.MapGet("/songs/search", async (string title, Supabase.Client client) =>
{
    var response = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)")
    .Filter(song => song.Title, Constants.Operator.ILike, "%" + title + "%").Get();

    var song = response.Models.FirstOrDefault();

    if (song is null)
    {
        return Results.NotFound();
    }

    var songResponse = new SongResponse
    {
        Title = song.Title,
        Album = song.Album.Title,
        Artist = song.Artist.Name,
        Lyrics = song.Lyrics
    };

    return Results.Ok(songResponse);

});

//fetching multiple songs with same name
app.MapGet("/songs/multisearch", async (string title, Supabase.Client client) =>
{
    var response = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)")
    .Filter(song => song.Title, Constants.Operator.ILike, "%" + title + "%").Get();

   
    if (!response.Models.Any())
    {
        return Results.NotFound();
    }

    var songResponse = response.Models.Select(song => new SongResponse
    {
        Title = song.Title,
        Album = song.Album.Title,
        Artist = song.Artist.Name,
        Lyrics = song.Lyrics
    });

    return Results.Ok(songResponse);

});

// fetch single artist
app.MapGet("artists/search", async (string name, Supabase.Client client) =>
{
    var response = await client.From<Artists>().Select("*")
    .Filter(artist => artist.Name, Constants.Operator.ILike, "%" + name + "%").Get();

    var artist = response.Models.FirstOrDefault();

    if(artist is null)
    {
        return Results.NotFound();
    }

    var artistResponse = new ArtistResponse
    {
        Name = artist.Name,
        Bio = artist.Bio
    };

    return Results.Ok(artistResponse);
});

// fetch multiple artists
app.MapGet("/artists/multisearch", async (string name, Supabase.Client client) =>
{
    var response = await client.From<Artists>().Select("*")
    .Filter(artist => artist.Name, Constants.Operator.ILike, "%" + name + "%").Get();


    if (!response.Models.Any())
    {
        return Results.NotFound();
    }

    var artistResponse = response.Models.Select(artist => new ArtistResponse
    {
        Name= artist.Name
    });

    return Results.Ok(artistResponse);

});

// fetch single album
app.MapGet("/albums/search", async (string title, Supabase.Client client) =>
{
    var response = await client.From<Albums>().Select("*, artist:artists(name)")
    .Filter(album => album.Title, Constants.Operator.ILike, "%" + title + "%").Get();

    var album = response.Models.FirstOrDefault();

    if (album is null)
    {
        return Results.NotFound();
    }

    var albumResponse = new AlbumResponse
    {
        Title = album.Title,
        Artist = album.Artist.Name,
        Genre = album.Genre,
        ReleaseDate = album.ReleaseDate
    };

    return Results.Ok(albumResponse);

});

// fetch multi albums
app.MapGet("/albums/multisearch", async (string title, Supabase.Client client) =>
{
    var response = await client.From<Albums>().Select("*, artist:artists(name)")
    .Filter(album => album.Title, Constants.Operator.ILike, "%" + title + "%").Get();


    if (!response.Models.Any())
    {
        return Results.NotFound();
    }

    var albumResponse = response.Models.Select(album => new AlbumResponse
    {
        Title = album.Title,
        Artist = album.Artist.Name,
        Genre = album.Genre,
        ReleaseDate = album.ReleaseDate
    });

    return Results.Ok(albumResponse);

});

// adding a song in favourites - broken
app.MapPost("/favorites", async (CreateFavoriteRequest request, Supabase.Client client) =>
{
    var userResponse = await client.From<Users>().Select("*").Filter("username", Constants.Operator.Equals, request.Username).Get();
    var user = userResponse.Models.FirstOrDefault();

    if(user is null)
    {
        return Results.BadRequest("User not found");
    }

    var songResponse = await client.From<Songs>().Select("*").Filter("title", Constants.Operator.Equals, request.SongTitle).Get();

    var song = songResponse.Models.FirstOrDefault();

    if (song is null)
    {
        return Results.BadRequest("Song not found");
    }

    var favorite = new Favorites
    {
        UserId = user.Id,
        SongId = song.Id,
        AddedAt = DateTime.UtcNow
    };

    var response = await client.From<Favorites>().Insert(favorite);
    var createdFav = response.Models.FirstOrDefault();

    if(createdFav is null)
    {
        return Results.BadRequest("Failed to add the song in favorites");
    }

    return Results.Created("Song added to favorites!", new FavoriteResponse
    {
        Id = createdFav.Id,
        UserId = createdFav.UserId,
        SongId = createdFav.SongId,
        AddedAt = createdFav.AddedAt
    });
}

);

// fetching all favorites - broken
app.MapGet("/favorites/{userId}", async (long userId, Supabase.Client client) =>
{
    var response = await client.From<Favorites>()
    .Select("*, song:songs(title, album:albums(title), artist:artists(name))")
    .Filter("user_id", Constants.Operator.Equals, userId).Get();
    
    if(!response.Models.Any())
    {
        return Results.NotFound("This user's favorite list is empty!");
    }

    var favoriteResponse = response.Models.Select(favorite => new FavoriteFetchResponse
    {
        SongTitle = favorite.Song.Title,
        AlbumName = favorite.Song.Album.Title,
        ArtistName = favorite.Song.Artist.Name
    });

    return Results.Ok(favoriteResponse);

});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
