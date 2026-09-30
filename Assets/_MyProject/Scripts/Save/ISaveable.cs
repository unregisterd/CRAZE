/// <summary>
/// 需要存读数据的对象实现这个接口。
/// SaveManager 会遍历所有实现者，统一收集和分发数据。
/// </summary>
public interface ISaveable
{
    /// <summary>把这个对象的数据写进 SaveData。</summary>
    void OnSave(SaveData data);

    /// <summary>从 SaveData 读回这个对象的数据。</summary>
    void OnLoad(SaveData data);
}