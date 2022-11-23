using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Roswell Doria
/// Date: 2022-11-10
/// 
/// The purpose of this Network Behavior is to monitor the PlayerITem Object that is spawned by the Network Runner
/// on player joined. This object observes when both players select a character. Once both have selected a character
/// spawn the associated character prefabs and start GameManager countdown.
/// </summary>
public class PlayerItemObserver : NetworkBehaviour
{
    public static PlayerItemObserver Observer = null;

    // other scene objects to reference
    protected GameManager _gameManager;

    [SerializeField] private NetworkObject[] CharacterPrefabs;

    private bool isPlayerOneReady = false;
    private bool isPlayerTwoReady = false;

    //default
    private int playerOneIndexSelect = 0;
    private int playerTwoIndexSelect = 0;

    //the fighters they will spawn
    private NetworkObject playerOneFighter;
    private NetworkObject playerTwoFighter;

    //default
    private int playerOneRef = 0;
    private int playerTwoRef = 0;

    private bool isPlayersSpawned = false;


    // Awake is called when the script instance is being loaded
    public void Awake()
    {
        Observer = this;
        Debug.Log("PlayerItemObserver instance awake: " + Observer);
    }


    // Start is called after Awake, and before Update
    public void Start()
    {
        CacheOtherObjects();
    }

    // Helper method to initialize OTHER game objects and their components
    private void CacheOtherObjects()
    {
        if (!_gameManager) _gameManager = GameManager.Manager;
    }


    public override void FixedUpdateNetwork()
    {

        // Both players are ready (selected their character)
        if (isPlayerOneReady && isPlayerTwoReady && Runner.IsServer && !isPlayersSpawned)
        {
            // Spawn the game stage
            GameStageController.Instance.RPC_SelectRandomStage();

            // Despawn Player one and player two character select objects
            RPC_DespawnPlayerItems(playerOneRef, playerTwoRef);
            // Spawn Player one and player two selected characters
            RPC_SpawnNetworkFighters(playerOneIndexSelect, playerTwoIndexSelect, playerOneRef, playerTwoRef);
            isPlayersSpawned = true;

            // Assign Player one and player two references to GameManager
            _gameManager.RPC_CachePlayers(playerOneRef, playerOneFighter, playerTwoRef, playerTwoFighter);


            // Switch Game State to 'Starting' Game
            _gameManager.RPC_SetGameStateStarting();
        }
    }

    /// <summary>
    /// Author: Roswell Doria
    /// Date: 2022-11-10
    /// 
    /// RPC respsonible for setting players to the ready state.
    /// </summary>
    /// <param name="playerRefIndex">an interger representing the playerRef Index of player one</param>
    /// <param name="playerPrefabIndex">an interger representing the playerRef Index of player two</param>
    /// <param name="isHost">a bool if player is host</param>
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerReady(int playerRefIndex, int playerPrefabIndex, bool isHost)
    {
        if (isHost)
        {
            playerOneRef = playerRefIndex;
            playerOneIndexSelect = playerPrefabIndex;
            isPlayerOneReady = true;
        }
        else if (!isHost)
        {
            playerTwoRef = playerRefIndex;
            playerTwoIndexSelect = playerPrefabIndex;
            isPlayerTwoReady = true;
        }

    }


    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CheckBothPlayerReady()
    {

    }

    /// <summary>
    /// Author: Roswell Doria
    /// Date: 2022-11-10
    /// 
    /// This RPC is responsible for dispawning the PlayerItems that are responsible for character select.
    /// </summary>
    /// <param name="playerOneRef">an interger representing the playerRef Index of player one</param>
    /// <param name="playerTwoRef">an interger representing the playerRef Index of player two</param>
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_DespawnPlayerItems(int playerOneRef, int playerTwoRef)
    {
        //Obtain PlayerItem Network Object
        NetworkObject playerOnePlayerItem = Runner.GetPlayerObject(playerOneRef);
        NetworkObject playerTwoPlayerItem = Runner.GetPlayerObject(playerTwoRef);
        //Despawn PlayerItem from runner
        Runner.Despawn(playerOnePlayerItem);
        Runner.Despawn(playerTwoPlayerItem);
    }

    /// <summary>
    /// Author: Roswell Doria
    /// Date: 2022-11-10
    /// 
    /// This Rpc method is to be used to spawn player one and player two objects and assign the associated player references.
    /// </summary>
    /// <param name="playerOneSelected"></param>
    /// <param name="playerTwoSelected"></param>
    /// <param name="playerOneRef"></param>
    /// <param name="playerTwoRef"></param>
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SpawnNetworkFighters(int playerOneSelected, int playerTwoSelected, int playerOneRef, int playerTwoRef)
    {
        // Player Spawn points
        Vector3 playerOneSpawnLocation = new Vector3(1, 0, 0);
        Vector3 playerTwoSpawnLocation = new Vector3(-1, 0, 0);
        // Spawn players
        playerOneFighter = this.Runner.Spawn(CharacterPrefabs[playerOneSelected], playerOneSpawnLocation, Quaternion.identity, playerOneRef);
        playerTwoFighter = this.Runner.Spawn(CharacterPrefabs[playerTwoSelected], playerTwoSpawnLocation, Quaternion.identity, playerTwoRef);

    }

}
