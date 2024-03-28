public struct BucketModel
{
    public string Token;
    public string Password;

    public BucketModel(string token, string password)
    {
        Token = token;
        Password = password;
    }
}

public struct LoadModel
{
    public string Bucket;
    public string Password;

    public LoadModel(string bucket, string password)
    {
        Bucket = bucket;
        Password = password;
    }
}

public struct SaveModel
{
    public string Bucket;
    public string Password;

    public SaveModel(string bucket, string password)
    {
        Bucket = bucket;
        Password = password;
    }
}

public struct UploadModel
{
    public string Data;
    public string Password;

    public UploadModel(string data, string password)
    {
        Data = data;
        Password = password;
    }
}

