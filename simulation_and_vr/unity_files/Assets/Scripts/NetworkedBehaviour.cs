using UnityEngine;
using Ubiq.Messaging;

/// <summary>
/// 这是专门为你补全的基类文件，用于连接 Ubiq 网络层与实验逻辑。
/// </summary>
public abstract class NetworkedBehaviour : MonoBehaviour
{
    // 提供给 LobbyManager 和 TrialSyncManager 用来发送消息的变量
    protected NetworkContext context;
    protected bool isNetworkActive = false;

    protected virtual void Awake()
    {
        var ns = NetworkScene.Find(this);
        if (ns == null)
        {
            Debug.LogError("【网络致命错误】在当前场景中找不到 NetworkScene！请确保从 Demo 场景复制了该物体并放在层级根部。");
            isNetworkActive = false;
            return;
        }

        // 自动向 Ubiq 的 NetworkScene 注册自己
        context = NetworkScene.Register(this);
        isNetworkActive = true;
    }

    // 作为一个虚方法，允许子类（LobbyManager等）重写它来接收网络消息
    public virtual void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // 基类中默认不处理，留给具体的子类去写逻辑
    }
}