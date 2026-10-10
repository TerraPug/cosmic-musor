using System.Collections.Generic;
using UnityEngine;

// Создаёт заменяемый набор прототипных объектов, не меняя исходные объекты сцены.
[DefaultExecutionOrder(-1000)]
public sealed class PlaceholderBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject[] replacedObjects;
    [SerializeField] private Material surfaceMaterial;
    [SerializeField, Range(3, 100)] private int garbageCount = 18;
    [SerializeField, Min(3f)] private float fieldRadius = 12f;
    [SerializeField, Min(0.1f)] private float playerMass = 80f;
    [SerializeField, Min(0.1f)] private float pickupRange = 4f;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, -8f);
    [SerializeField] private Vector3 stationPosition = new Vector3(0f, 0f, 12f);

    private readonly List<Material> materials = new List<Material>();
    private readonly List<GameObject> disabledObjects = new List<GameObject>();
    private Transform generatedRoot;
    private TwoHandGarbageCollector collector;
    private TrashController station;
    private GUIStyle hudStyle;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
        if (surfaceMaterial == null)
        {
            Debug.LogError("Для генератора заглушек не назначен материал поверхности.", this);
            enabled = false;
            return;
        }

        if (replacedObjects != null)
            foreach (GameObject original in replacedObjects)
                if (original != null && original.activeSelf)
                {
                    disabledObjects.Add(original);
                    original.SetActive(false);
                }

        generatedRoot = new GameObject("Generated placeholders").transform;
        generatedRoot.SetParent(transform, false);
        Material metal = MakeMaterial(new Color(0.22f, 0.3f, 0.4f));
        Material orange = MakeMaterial(new Color(1f, 0.48f, 0.08f));
        Material cyan = MakeMaterial(new Color(0.08f, 0.8f, 0.9f));
        CreatePlayer(metal, orange);
        CreateStation(metal, cyan);
        CreateGarbage(metal, orange, cyan);
        Debug.Log($"Созданы заглушки: игрок, станция и {garbageCount} предметов мусора.", this);
    }

    private Material MakeMaterial(Color color)
    {
        Material material = new Material(surfaceMaterial) { color = color };
        materials.Add(material);
        return material;
    }

    private Transform CreateRoot(string objectName, Vector3 position)
    {
        Transform root = new GameObject(objectName).transform;
        root.SetParent(generatedRoot, false);
        root.localPosition = position;
        return root;
    }

    private GameObject Shape(string objectName, PrimitiveType type, Transform parent,
        Vector3 position, Vector3 scale, Material material, bool solid = true)
    {
        GameObject shape = GameObject.CreatePrimitive(type);
        shape.name = objectName;
        shape.transform.SetParent(parent, false);
        shape.transform.localPosition = position;
        shape.transform.localScale = scale;
        shape.GetComponent<Renderer>().sharedMaterial = material;
        if (!solid)
        {
            Collider collider = shape.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
        }
        return shape;
    }

    private void CreatePlayer(Material metal, Material orange)
    {
        Transform player = CreateRoot("Placeholder player", spawnPosition);
        player.gameObject.tag = "Player";
        CapsuleCollider collider = player.gameObject.AddComponent<CapsuleCollider>();
        collider.radius = 0.45f;
        collider.height = 1.8f;
        collider.center = new Vector3(0f, -0.4f, 0f);
        Rigidbody body = player.gameObject.AddComponent<Rigidbody>();
        body.mass = playerMass;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        player.gameObject.AddComponent<SpacePlayerController>();
        collector = player.gameObject.AddComponent<TwoHandGarbageCollector>();
        collector.pickupRange = pickupRange;
        collector.leftkleshnya = CreateHand(player, -1f, metal, orange);
        collector.rightkleshnya = CreateHand(player, 1f, metal, orange);
        Shape("Suit body", PrimitiveType.Cube, player, new Vector3(0f, -0.65f, -0.25f),
            new Vector3(0.75f, 1.1f, 0.6f), metal, false);

        Transform cameraTransform = new GameObject("Placeholder camera").transform;
        cameraTransform.SetParent(player, false);
        cameraTransform.localPosition = new Vector3(0f, 0.2f, 0.05f);
        cameraTransform.gameObject.tag = "MainCamera";
        Camera camera = cameraTransform.gameObject.AddComponent<Camera>();
        camera.fieldOfView = 75f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 300f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.008f, 0.015f, 0.035f);
        cameraTransform.gameObject.AddComponent<AudioListener>();
    }

    private Transform CreateHand(Transform player, float side, Material metal, Material orange)
    {
        Transform hand = new GameObject(side < 0 ? "Left claw" : "Right claw").transform;
        hand.SetParent(player, false);
        hand.localPosition = new Vector3(side * 0.65f, -0.35f, 1.6f);
        Shape("Arm", PrimitiveType.Cube, hand, new Vector3(0f, 0f, -0.6f),
            new Vector3(0.18f, 0.18f, 0.9f), metal, false);
        for (int i = -1; i <= 1; i += 2)
            Shape("Claw finger", PrimitiveType.Cube, hand, new Vector3(i * 0.2f, 0f, -0.1f),
                new Vector3(0.1f, 0.2f, 0.5f), orange, false);
        return hand;
    }

    private void CreateStation(Material metal, Material cyan)
    {
        Transform root = CreateRoot("Placeholder recycling station", stationPosition);
        // Открытый приёмник: физическая рама не перекрывает вход в триггер.
        Shape("Left wall", PrimitiveType.Cube, root, new Vector3(-2.5f, 0f, 0f), new Vector3(0.5f, 5f, 4f), metal);
        Shape("Right wall", PrimitiveType.Cube, root, new Vector3(2.5f, 0f, 0f), new Vector3(0.5f, 5f, 4f), metal);
        Shape("Floor", PrimitiveType.Cube, root, new Vector3(0f, -2.5f, 0f), new Vector3(5.5f, 0.5f, 4f), metal);
        Shape("Roof", PrimitiveType.Cube, root, new Vector3(0f, 2.5f, 0f), new Vector3(5.5f, 0.5f, 4f), metal);
        Shape("Back", PrimitiveType.Cube, root, new Vector3(0f, 0f, 2f), new Vector3(5f, 5f, 0.4f), metal);
        Shape("Intake marker", PrimitiveType.Cube, root, new Vector3(0f, 2.5f, -2.1f), new Vector3(5f, 0.18f, 0.18f), cyan, false);
        Transform intake = new GameObject("Release garbage here").transform;
        intake.SetParent(root, false);
        BoxCollider trigger = intake.gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(4.4f, 4.4f, 3.6f);
        station = intake.gameObject.AddComponent<TrashController>();
    }

    private void CreateGarbage(Material metal, Material orange, Material cyan)
    {
        Material[] palette = { metal, orange, cyan };
        PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
        for (int i = 0; i < garbageCount; i++)
        {
            // Фиксированная раскладка не меняет глобальный Random и оставляет вход станции свободным.
            float angle = i * 2.399963f;
            float radius = fieldRadius * Mathf.Sqrt((i + 1f) / garbageCount);
            Vector3 position = i < 2
                ? spawnPosition + new Vector3(i == 0 ? -1f : 1f, 0f, 2.8f)
                : new Vector3(Mathf.Cos(angle) * radius, ((i % 5) - 2) * 0.7f, Mathf.Sin(angle) * radius * 0.5f);
            int kind = i % types.Length;
            Vector3 scale = kind == 0 ? new Vector3(0.65f, 0.4f, 0.85f)
                : kind == 1 ? Vector3.one * 0.65f : new Vector3(0.5f, 0.45f, 0.5f);
            GameObject item = Shape($"Scrap {i + 1:00}", types[kind], generatedRoot, position, scale, palette[kind]);
            Rigidbody body = item.AddComponent<Rigidbody>();
            body.mass = kind == 0 ? 3f : kind == 1 ? 1f : 2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            item.AddComponent<GarbageItem>();
        }
    }

    private void OnGUI()
    {
        if (collector == null || station == null) return;
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, wordWrap = true };
            crosshairStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        }
        hudStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.024f), 12, 28);
        crosshairStyle.fontSize = hudStyle.fontSize;
        GUI.Box(new Rect(Screen.width * 0.02f, Screen.height * 0.02f, Screen.width * 0.52f, Screen.height * 0.2f),
            $"WASD — полёт | Space / Ctrl — вверх / вниз\nМышь — обзор | Esc — курсор\nУдерживай ЛКМ / ПКМ — захват; отпусти в станции\nЛевая: {(collector.LeftHandFree ? "свободна" : "занята")} | Правая: {(collector.RightHandFree ? "свободна" : "занята")}\nСдано: {station.DeliveredCount} / {garbageCount}", hudStyle);
        GUI.Label(new Rect(Screen.width * 0.48f, Screen.height * 0.48f, Screen.width * 0.04f, Screen.height * 0.04f), "+", crosshairStyle);
    }

    private void OnDestroy()
    {
        if (generatedRoot != null) Destroy(generatedRoot.gameObject);
        foreach (Material material in materials)
            if (material != null) Destroy(material);
        materials.Clear();
        foreach (GameObject original in disabledObjects)
            if (original != null) original.SetActive(true);
        disabledObjects.Clear();
    }
}
