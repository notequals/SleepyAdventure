﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public static float time { get { return Time.time + 60; } }

    public GameObject inventoryMenu;

    public GameObject playerPrefab;

    public PlayerMovement player;
    public Transform hud;

    public Dictionary<int, Unit> units = new Dictionary<int, Unit>();
    public Dictionary<int, Spell> projectiles = new Dictionary<int, Spell>();
    public Dictionary<int, Obstacle> obstacles = new Dictionary<int, Obstacle>();

    private int unitIDCounter = 0;

    [System.NonSerialized]
    public int gameCounter = 0;

    GameObject levelHolder;
    Level level;

    DamageTextController damageTextManager;
    EmojiBarManager emojiBarManager;

    PrefabManager prefabManager;

    private bool gameOver = false;

    public AttackButton attackButton;
        
    public delegate void Callback();
    public event Callback OnGameStart;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Use this for initialization
    void Start()
    {
        level = new Level();
        levelHolder = new GameObject("LevelHolder");

        damageTextManager = GetComponent<DamageTextController>();
        emojiBarManager = GetComponent<EmojiBarManager>();

        prefabManager = PrefabManager.instance;        
    }

    // Update is called once per frame
    void Update()
    {
        DeleteDeadMonsters();
        UpdateWalkableSquares();
        gameCounter += 1;

        DelayAction.OnUpdate();
    }

    void DeleteDeadMonsters()
    {        
        foreach (var key in units.Keys.ToList())
        {
            var unit = units[key];

            if(unit != null && unit is Monster)
            {
                var monster = unit as Monster;

                if(monster.health <= 0)
                {
                    monster.Death();
                }
            }
        }
    }

    void UpdateWalkableSquares()
    {
        /*foreach(var square in level.map.Values)
        {
            square.obstacles = new Dictionary<int, Entity>();
        }*/

        foreach(var unit in units.Values)
        {
            var square = level.GetSquareAtPoint(unit.transform.position);
            if (square != null)
            {                
                if(unit.sqr != square)
                {
                    unit.sqr.obstacles.Remove(unit.id);

                    unit.sqr = square;
                    unit.sqr.obstacles.Add(unit.id, unit);

                    unit.pos2d = square.position;
                }

            }
        }
    }

    public bool SameDestination(Unit current, IPosition pos)
    {
        foreach(var unit in units.Values)
        {
            if(current != unit && unit.nextPos == pos)
            {
                return true;
            }
        }

        return false;
    }

    public void LoadLevel(string path)
    {
        level = new Level();

        string str = File.ReadAllText(path);

        var sqrObjects = JsonHelper.FromJson<SquareObject>(str);

        foreach (var obj in sqrObjects)
        {
            IPosition pos2d = obj.pos.To2D();

            Square square = level.GetSquareAtPoint(pos2d);
            
            if(square == null)
            {
                square = new Square(pos2d);
                level.map.Add(square.position, square);
            }
            square.objects.Add(obj);
                                    
            if (obj != null)
            {
                var newObject = CreateNewObject(obj.pid, obj.pos, obj.rotation);
                obj.SetGameObject(newObject);
                                
                var entityScript = newObject.GetComponent<Entity>();
                if (entityScript != null)
                {
                    entityScript.id = unitIDCounter;
                    entityScript.sqr = square;

                    entityScript.pos2d = square.position;                    
                }

                var unitScript = newObject.GetComponent<Unit>();
                if (unitScript != null)
                {
                    CreateUnit(unitScript);
                }

                var obstacleScript = newObject.GetComponent<Obstacle>();
                if(obstacleScript != null)
                {
                    CreateObstacle(obstacleScript);
                }
            }
        }

        InitLevel();

    }

    /*public void CreateParticle(Unit unit, GameObject particle, Vector3 position)
    {
        Instantiate(particle, new Vector3(pos.x, pos.y / 2.0f, pos.z), Quaternion.Euler(rotation), levelHolder.transform);
    }*/

    public void CreateUnit(Unit unit)
    {
        unit.sqr.obstacles.Add(unitIDCounter, unit);
        units.Add(unitIDCounter, unit);
        unitIDCounter+= 1;
    }

    public void DeleteUnit(Unit unit, float time)
    {
        unit.sqr.obstacles.Remove(unit.id);
        unit.collider.isTrigger = true;

        units.Remove(unit.id);

        if (time != 0)
        {
            Destroy(unit.gameObject, time);
        }
        else
        {
            Destroy(unit.gameObject);
        }
    }

    public void CreateObstacle(Obstacle obj)
    {
        obj.sqr.obstacles.Add(unitIDCounter, obj);
        obstacles.Add(unitIDCounter, obj);
        unitIDCounter += 1;
    }

    void InitLevel()
    {
        Pathfinding.InitPathSquares(level);
        hud.gameObject.SetActive(true);
        Inventory.instance.equipment.SetDefaultWeapon();

        if (OnGameStart != null)
        {
            OnGameStart();
        }
    }

    GameObject CreateNewObject(int pid, IPosition pos, Vector3 rotation)
    {
        var selectedOriginal = prefabManager.GetGameObject(pid);

        if (selectedOriginal.tag == "Start")
        {
            selectedOriginal = playerPrefab;
        }

        var newObject = Instantiate(selectedOriginal, new Vector3(pos.x, pos.y / 2.0f, pos.z), Quaternion.Euler(rotation), levelHolder.transform);

        if (newObject.tag == "Player")
        {
            player = newObject.GetComponent<PlayerMovement>();
        }

        return newObject;
    }

    public PathInfo UnitMoveTo(Unit unit, Vector3 to)
    {
        return UnitMoveTo(unit, unit.transform.position, to);
    }

    public PathInfo UnitMoveTo(Unit unit, Vector3 from, Vector3 to)
    {
        var from2d = from.ConvertToIPosition().To2D();
        var to2d = to.ConvertToIPosition().To2D();

        if (from2d.Distance(to2d) < 2)
        {
            var interactable = level.GetInteractableObject(to2d);

            if (interactable != null)
            {
                if (interactable.Use(unit))
                {
                    unit.DisableUnitMovement(.5f);
                    return null;
                }
            }
        }

        return Pathfinding.GetPath(unit, from, to);
    }

    public void SetScene(string str, float waitTime)
    {
        DelayAction.Add(() => SetScene(str), waitTime);
    }

    public void SetScene(string str)
    {
        SceneManager.LoadScene(str);
    }

    public void CreateDamageText(Unit unit, int damage)
    {
        damageTextManager.CreateDamageText(unit, damage);
    }

    public EmojiBar CreateEmojiBar(Monster unit)
    {
        return emojiBarManager.CreateBar(unit);
    }

    public void CreateCircularSpell(Unit source, CircularSpell spell, Vector3 pos)
    {
        var aoe = Instantiate(spell, pos, Quaternion.identity);

        aoe.source = source;
    }

    public void CreateLinearSpell(Unit source, LinearSpell projectile, Vector3 from, Vector3 to)
    {
        var proj = Instantiate(projectile, from, Quaternion.identity);

        proj.source = source;
        proj.start = from;
        proj.end = to;

        proj.SetVelocity();
    }

    public Vector3 GetMousePosition()
    {
        if (player)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var groundPlane = new Plane(player.transform.up, player.transform.position);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                return ray.GetPoint(rayDistance);
            }
        }

        return Vector3.zero;
    }

    public Vector3 GetTouchPosition(Vector2 position)
    {
        if (player)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            var groundPlane = new Plane(player.transform.up, player.transform.position);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                return ray.GetPoint(rayDistance);
            }
        }

        return Vector3.zero;
    }

    public void GameOver()
    {
        gameOver = true;
        Camera.main.GetComponent<CameraFollow>().isFollowing = true;

        if(player != null)
        {
            player.Stop();
        }
    }
}
