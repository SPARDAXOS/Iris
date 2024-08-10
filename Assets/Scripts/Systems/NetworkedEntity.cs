using Unity.Netcode;
using static MyUtility.Utility;



public abstract class NetworkedEntity : NetworkBehaviour {

    protected bool initialized = false;
    protected GameInstance gameInstanceRef = null;

    /// <summary>
    /// Should be called at entity instantiation.
    /// <para>  -Check if 'initialized' is TRUE at start then return if so    </para>
    /// <para>  -Set "gameInstanceRef" to passed argument 'game'              </para>
    /// <para>  -Set 'initialized' to true at the end of the function         </para>
    /// </summary>
    public abstract void Initialize(GameInstance game);

    /// <summary>
    /// Called instead of Update by MonoBehaviour.
    /// <para>  -Check if 'initialized' is TRUE at start then return if it is FALSE and print a message using Utility.Error  </para>
    /// </summary>
    public abstract void Tick();

    /// <summary>
    /// Called instead of FixedUpdate by MonoBehaviour.
    /// <para>  -Check if 'initialized' is TRUE at start then return if it is FALSE and print a message using Utility.Error  </para>
    /// </summary>
    public virtual void FixedTick() {
        Warning("Fixed tick called on an entity that does not provide implementation for FixedTick!");
    }

    /// <summary>
    /// Called to clean up resources allocated by this entity.
    /// </summary>
    public virtual void CleanUp(string message = "Entity cleaned up successfully!") {
        //The order of which Monobehaviors get destroyed is out of my control.
        //-Thus i cannot use this.name.
        if (gameInstanceRef.IsDebuggingEnabled())
            Log(message);
    }
}
