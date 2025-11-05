using System.Text.Json;

namespace Infrastructure.FirebaseDatabase;

//класс для работы для интеграции энтити в бдшку
public class FirebaseRepository
{
    private readonly FirebaseService _firebaseService;

    public FirebaseRepository(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task SetAsync<T>(string path, T entity, IFirebaseListener listener)
    {
        if (entity != null)
        {
            var result = await _firebaseService.PutAsync(path, entity);
            if (result.IsSuccess)
                listener.OnSuccess($"Entity saved to '{path}'");
            else
                listener.OnError(result.Message);
        }
    }

    public async Task GetAsync<T>(string path, IFirebaseListener listener)
    {
        var result = await _firebaseService.GetAsync(path);
        if (!result.IsSuccess)
        {
            listener.OnError(result.Message);
            return;
        }

        try
        {
            var data = JsonSerializer.Deserialize<T>(result.RawJson!);
            listener.OnDataSnapshot(data!);
        }
        catch
        {
            listener.OnError("Failed to parse snapshot");
        }
    }

    public async Task DeleteAsync(string path, IFirebaseListener listener)
    {
        var result = await _firebaseService.DeleteAsync(path);
        if (result.IsSuccess)
            listener.OnSuccess($"Deleted {path}");
        else
            listener.OnError(result.Message);
    }
}