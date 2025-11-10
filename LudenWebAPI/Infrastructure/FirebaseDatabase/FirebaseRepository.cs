using System.Text.Json;

namespace Infrastructure.FirebaseDatabase;

// Класс для интеграции энтити в Firebase
public class FirebaseRepository
{
    private readonly FirebaseService _firebaseService;

    public FirebaseRepository(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<FirebaseResult> SetAsync<T>(string path, T entity, IFirebaseListener listener)
    {
        if (entity == null)
        {
            listener.OnError("Entity is null");
            return new FirebaseResult { IsSuccess = false, Message = "Entity is null" };
        }

        var result = await _firebaseService.PutAsync(path, entity);

        if (result.IsSuccess)
        {
            listener.OnSuccess($"Entity saved to '{path}'");
        }
        else
        {
            listener.OnError(result.Message);
        }

        return result;
    }

    public async Task<FirebaseResult> GetAsync<T>(string path, IFirebaseListener listener)
    {
        var result = await _firebaseService.GetAsync(path);
        if (!result.IsSuccess)
        {
            listener.OnError(result.Message);
            return result;
        }

        try
        {
            var data = JsonSerializer.Deserialize<T>(result.RawJson!);
            if (data != null)
                listener.OnDataSnapshot(data);
            else
                listener.OnError("Received null data snapshot");
        }
        catch (Exception ex)
        {
            listener.OnError($"Failed to parse snapshot: {ex.Message}");
        }

        return result;
    }

    public async Task<FirebaseResult> DeleteAsync(string path, IFirebaseListener listener)
    {
        var result = await _firebaseService.DeleteAsync(path);
        if (result.IsSuccess)
        {
            listener.OnSuccess($"Deleted '{path}'");
        }
        else
        {
            listener.OnError(result.Message);
        }

        return result;
    }
}