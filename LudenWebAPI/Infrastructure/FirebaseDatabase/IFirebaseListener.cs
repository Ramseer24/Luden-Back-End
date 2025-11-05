namespace Infrastructure.FirebaseDatabase;

public interface IFirebaseListener
{
    void OnSuccess(string message);
    void OnError(string reason);
    void OnDataSnapshot<T>(T data);
}