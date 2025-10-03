using LyricsBackend.Contracts;
using LyricsBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest;
using System.Linq.Expressions;
using System.Xml.Linq;

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
        if (foundExistingUser.Username == user.Username && foundExistingUser.Email == user.Email)
        {
            return Results.Conflict("Username and Email already exist");
        }

        if (foundExistingUser.Username == user.Username)
        {
            return Results.Conflict("Username already exists");
        }

        return Results.Conflict("Email already exists");
    }


    var response = await client.From<Users>().Insert(user);

    var newUser = response.Models.First();

    var userResponse = new UserResponse
    {
        Id = newUser.Id,
        Username = newUser.Username,
        Password = newUser.Password,
        Email = newUser.Email,
        CreatedAt = newUser.CreatedAt
    };

    return Results.Created("User created successfully",userResponse);
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

// login check
app.MapGet("/login", async (string username, string password, Supabase.Client client) =>
{
    var response = await client.
    From<Users>().Where(n => n.Username == username && n.Password == password).Get();

    var user = response.Models.FirstOrDefault();

    if (user is null)
    {
        return Results.NotFound("Invalid username or password");
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

//change pass
app.MapPatch("/users/passchange", async (PassChangeRequest request, Supabase.Client client) =>
{
    var user = await client.From<Users>().Where(u => u.Id == request.UserId).Single();

    if (user is null)
    {
        return Results.NotFound("User not found");
    }

    user.Password = request.NewPassword;

    await user.Update<Users>();

    return Results.Ok("Password changed successfully");
});

//fetching a song
app.MapGet("/songs/search", async (string title, string artist, Supabase.Client client) =>
{
    var artistResponse = await client.From<Artists>().Select("*")
    .Filter(artist => artist.Name, Constants.Operator.ILike, "%" + artist + "%").Single();

    if (artistResponse is null)
    {
        return Results.NotFound("No song found from that artist");
    }

    var songResponse = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)")
    .Filter(song => song.Title, Constants.Operator.ILike, "%" + title + "%")
    .Where(song => song.ArtistId == artistResponse.Id).Single();

    if (songResponse is null)
    {
        return Results.NotFound("Song not found");
    }

    var lyrics = await client.From<Lyrics>().Select("*, song:songs(id)").Where(lyric => lyric.SongId == songResponse.Id).Single();

    var singleSongRequest = new SongResponse
    {
        Id = songResponse.Id,
        Title = songResponse.Title,
        Album = songResponse.Album.Title,
        Artist = songResponse.Artist.Name,
        Lyrics = lyrics.Text
    };

    return Results.Ok(singleSongRequest);

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
        Id =song.Id,
        Title = song.Title,
        Album = song.Album.Title,
        Artist = song.Artist.Name
    });

    return Results.Ok(songResponse);

});

//fetching multiple songs with the same album
app.MapGet("/songs/albumsearch", async (string albumTitle, Supabase.Client client) =>
{
    var albumCheck = await client.From<Albums>().Select("*, artist:artists(name), genre:genres(name)")
    .Filter(album => album.Title, Constants.Operator.ILike, "%" + albumTitle + "%").Single();

    if (albumCheck is null)
    {
        return Results.NotFound("Album not found");
    }

    var response = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)").Where(s => s.AlbumId == albumCheck.Id).Get();

    if (!response.Models.Any())
    {
        return Results.NotFound();
    }

    var songResponse = response.Models.Select(song => new SongResponse
    {
        Id = song.Id,
        Title = song.Title,
        Album = song.Album.Title,
        Artist = song.Artist.Name
    });

    return Results.Ok(songResponse);

});

// fetch single artist
app.MapGet("artists/search", async (string name, string lang, Supabase.Client client) =>
{

    var artistFetch = await client.From<Artists>().Select("*")
    .Filter(artist => artist.Name, Constants.Operator.ILike, "%" + name + "%").Get();

    var artist = artistFetch.Models.FirstOrDefault();

    if (artist is null)
    {
        return Results.NotFound();
    }

    var bioFetch = await client.From<ArtistBio>().Select("*").Where(b => b.ArtistId == artist.Id && b.LanguageCode == lang).Single();

    ArtistResponse artistResponse = new ArtistResponse();
    if (bioFetch is null)
    {
        artistResponse = new ArtistResponse
        {
            Name = artist.Name,
            Bio = "Artist doesn't have a bio"
        };
        return Results.Ok(artistResponse);
    }

     artistResponse = new ArtistResponse
    {
        Name = artist.Name,
        Bio = bioFetch.Text
        
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
        Name = artist.Name
    });

    return Results.Ok(artistResponse);

});

// fetch single album
app.MapGet("/albums/search", async (string title, Supabase.Client client) =>
{
    var response = await client.From<Albums>().Select("*, artist:artists(name),genre:genres(name)")
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
        Genre = album.Genre.Name,
        ReleaseDate = album.ReleaseDate
    };

    return Results.Ok(albumResponse);

});

// fetch multi albums from the same artist
app.MapGet("/albums/multisearch", async (string artist, Supabase.Client client) =>
{
    var artistCheck = await client.From<Artists>().Select("*").Filter(artist => artist.Name, Constants.Operator.ILike, "%" + artist + "%").Single();

    if(artistCheck is null)
    {
        return Results.NotFound("Artist not found");
    }

    var response = await client.From<Albums>().Select("*, artist:artists(name), genre:genres(name)")
    .Where(album => album.ArtistId == artistCheck.Id).Order(album => album.ReleaseDate, Constants.Ordering.Descending).Get();


    if (!response.Models.Any())
    {
        return Results.NotFound();
    }

    var albumResponse = response.Models.Select(album => new AlbumResponse
    {
        Title = album.Title,
        Artist = album.Artist.Name,
        Genre = album.Genre.Name,
        ReleaseDate = album.ReleaseDate
    });

    return Results.Ok(albumResponse);

});

// adding a song in favourites - expects a json string with the username and the song title
app.MapPost("/favorites", async (CreateFavoriteRequest request, Supabase.Client client) =>
{
    var userResponse = await client.From<Users>().Select("*").Filter("username", Constants.Operator.Equals, request.Username).Get();
    var user = userResponse.Models.FirstOrDefault();

    if(user is null)
    {
        return Results.BadRequest("User not found");
    }

    var songResponse = await client.From<Songs>().Select("*").Where(x => x.Id == request.SongId).Get();

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

// fetching all favorites - gives the info about the song, album and artist in the form of a json string (to visualize the data kind of like spotify)
app.MapGet("/favorites/{userId}", async (long userId, Supabase.Client client) =>
{
    var response = await client.From<Favorites>()
    .Select("*").Where(fav => fav.UserId == userId).Get();
    
    if(!response.Models.Any())
    {
        return Results.NotFound("This user's favorite list is empty!");
    }

    List<FavoriteFetchResponse> songFetch = new List<FavoriteFetchResponse>();

    foreach (var favorite in response.Models)
    {
        var songCheck = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)").Where(s => s.Id == favorite.SongId).Get();

        var song = songCheck.Models.FirstOrDefault();
        if (song != null)
        {
            songFetch.Add(new FavoriteFetchResponse
            {
                SongId = song.Id,
                SongTitle = song.Title,
                AlbumName = song.Album.Title,
                ArtistName = song.Artist.Name
            });
        }
    }

    return Results.Ok(songFetch);

});

// single item check in favorites
app.MapGet("/favoritescheck", async (long userId, long songId, Supabase.Client client) =>
{

    var songResponse = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)").Where(s => s.Id == songId).Get();
    var song = songResponse.Models.FirstOrDefault();

    var favListCheck = await client.From<Favorites>().Select("*").Where(fav => (fav.UserId == userId) && (fav.SongId == songId)).Get();
    var favCheck = favListCheck.Models.FirstOrDefault();


    if (song is null)
    {
        return Results.NotFound("Song not found");
    }

    if (favCheck is null)
    {
        return Results.NotFound("Song not in favorites");
    }

    var favResponse = new FavoriteFetchResponse
    {
        SongId = song.Id,
        SongTitle = song.Title,
        AlbumName= song.Album.Title,
        ArtistName = song.Artist.Name
    };

    return Results.Ok(favResponse);

});

// removing a song from favorites
app.MapDelete("/favorites/delete/{userId}/{songId}", async (long userId, long songId, Supabase.Client client) =>
{
    var songResponse = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)").Where(s => s.Id == songId).Get();
    var song = songResponse.Models.FirstOrDefault();

    var favListCheck = await client.From<Favorites>().Select("*").Where(fav => (fav.UserId == userId) && (fav.SongId == song.Id)).Get();
    var favCheck = favListCheck.Models.FirstOrDefault();

    if (song != null && favCheck != null)
    {
        await client.From<Favorites>().Where(favSong => favSong.SongId == song.Id && favSong.UserId == userId).Delete();
        return Results.Ok("Removed song from favorites");
    }

    return Results.NotFound("No such song is in user's favorites");
});

//creating a history record of opened song
app.MapPost("/songhistory", async (CreateSongHistoryRequest request, Supabase.Client client) =>
{
    var userCheck = await client.From<Users>().Select("*").Where(u => u.Id == request.UserId).Get();
    var user = userCheck.Models.FirstOrDefault();

    if (user is null)
    {
        return Results.BadRequest("User not found");
    }

    var songCheck = await client.From<Songs>().Select("*, album:albums(title),artist:artists(name)").Where(s => s.Id == request.SongId).Get();
    var song = songCheck.Models.FirstOrDefault();

    if (song is null)
    {
        return Results.BadRequest("Song not found");
    }

    var record = new OpenedHistory
    {
        UserId = request.UserId,
        SongId = request.SongId,
        CreatedAt = DateTime.UtcNow
    };

    var existingCheck = await client.From<OpenedHistory>().Select("*")
    .Where(check => check.UserId == record.UserId && check.SongId == record.SongId).Get();
    var existing = existingCheck.Models.FirstOrDefault();

    if (existing != null)
    {
        return Results.Conflict("Record already created");
    }

    var response = await client.From<OpenedHistory>().Insert(record);
    var createdRecord = response.Models.FirstOrDefault();

    if (createdRecord is null)
    {
        return Results.BadRequest("Failed to create a record");
    }

    return Results.Created("Record created successfully", new CreateSongHistoryResponse
    {
        Id = createdRecord.Id,
        UserId = createdRecord.UserId,
        SongId = createdRecord.SongId,
        CreatedAt = createdRecord.CreatedAt
    });
});

//getting most recent five records of opened songs
app.MapGet("songhistory/recent", async(long userId, Supabase.Client client) =>
{
    var recentHistory = await client.From<OpenedHistoryFetch>().Select("id, created_at, song:songs(id, title, album:albums(title),artist:artists(name))")
    .Where(result => result.UserId == userId).Order(result => result.CreatedAt, Constants.Ordering.Descending).Limit(5).Get();

    if(recentHistory.Models.Count == 0)
    {
        return Results.NotFound("No recent history records found");
    }

    var result = recentHistory.Models.Select(song => new SongResponse
    {
        Id = song.Song.Id,
        Title = song.Song.Title,
        Album = song.Song.Album.Title,
        Artist = song.Song.Artist.Name
    });

    return Results.Ok(result);
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
