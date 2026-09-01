using UnityEngine;

public sealed class PrototypeWorld : MonoBehaviour
{
    private Material plaster;
    private Material floor;
    private Material asphalt;
    private Material grass;
    private Material vanPaint;

    private void Awake()
    {
        if (FindFirstObjectByType<JobManager>() != null) return;
        CreateMaterials();
        CreateLighting();
        GameObject systems = new GameObject("Job systems");
        systems.AddComponent<JobManager>();
        systems.AddComponent<JobHUD>();
        CreateEnvironment();
        CreateApartment();
        CreateVan();
        CreateCargo();
        CreateFirstPersonPlayer();
    }

    private void CreateMaterials()
    {
        plaster = Material(new Color(0.47f, 0.44f, 0.37f), 0f, 0.18f);
        floor = Material(new Color(0.22f, 0.17f, 0.13f), 0f, 0.12f);
        asphalt = Material(new Color(0.09f, 0.10f, 0.12f), 0f, 0.08f);
        grass = Material(new Color(0.13f, 0.22f, 0.12f), 0f, 0.05f);
        vanPaint = Material(new Color(0.05f, 0.28f, 0.48f), 0.28f, 0.38f);
    }

    private void CreateLighting()
    {
        RenderSettings.ambientLight = new Color(0.42f, 0.45f, 0.50f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.56f, 0.60f, 0.64f);
        RenderSettings.fogDensity = 0.0035f;
        GameObject sun = new GameObject("Overcast daylight");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.92f, 0.80f);
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
    }

    private void CreateEnvironment()
    {
        Block("Ground", new Vector3(0f, -0.35f, 0f), new Vector3(180f, 0.5f, 90f), grass);
        Block("Road", new Vector3(0f, -0.04f, 0f), new Vector3(180f, 0.12f, 14f), asphalt);
        Material marking = Material(new Color(0.84f, 0.75f, 0.42f), 0f, 0.1f);
        for (int x = -80; x <= 80; x += 10)
            Block("Road marking", new Vector3(x, 0.035f, 0f), new Vector3(5f, 0.02f, 0.15f), marking);

        GameObject destination = Block("Delivery zone", new Vector3(67f, 0.08f, 18f), new Vector3(15f, 0.15f, 13f), Material(new Color(0.12f, 0.75f, 0.35f), 0f, 0.15f));
        destination.GetComponent<BoxCollider>().isTrigger = true;
        destination.AddComponent<DeliveryZone>();
        Block("Warehouse back", new Vector3(74f, 3f, 27f), new Vector3(22f, 6f, 0.4f), Material(new Color(0.35f, 0.38f, 0.40f), 0.1f, 0.25f));
        Block("Warehouse side", new Vector3(84.8f, 3f, 20f), new Vector3(0.4f, 6f, 14f), plaster);
    }

    private void CreateApartment()
    {
        Vector3 o = new Vector3(-63f, 0f, 20f);
        Block("Apartment floor", o, new Vector3(22f, 0.3f, 18f), floor);
        Block("Back wall", o + new Vector3(0f, 2.7f, 8.8f), new Vector3(22f, 5.4f, 0.25f), plaster);
        Block("Left wall", o + new Vector3(-10.9f, 2.7f, 0f), new Vector3(0.25f, 5.4f, 18f), plaster);
        Block("Right wall", o + new Vector3(10.9f, 2.7f, 2.7f), new Vector3(0.25f, 5.4f, 12.6f), plaster);
        Block("Front left wall", o + new Vector3(-7.2f, 2.7f, -8.8f), new Vector3(7.5f, 5.4f, 0.25f), plaster);
        Block("Front right wall", o + new Vector3(7.2f, 2.7f, -8.8f), new Vector3(7.5f, 5.4f, 0.25f), plaster);

        Material stain = Material(new Color(0.16f, 0.14f, 0.10f), 0f, 0.05f);
        for (int i = 0; i < 8; i++)
        {
            GameObject mark = Block("Damp stain", o + new Vector3(-8.5f + i * 2.4f, 1.0f + i % 3, 8.65f), new Vector3(1.1f, 0.45f, 0.025f), stain);
            Destroy(mark.GetComponent<Collider>());
        }

        GameObject bulb = new GameObject("Bare ceiling bulb");
        bulb.transform.position = o + new Vector3(0f, 4.6f, 0f);
        Light point = bulb.AddComponent<Light>();
        point.type = LightType.Point;
        point.range = 13f;
        point.intensity = 4f;
        point.color = new Color(1f, 0.73f, 0.42f);
        point.shadows = LightShadows.Soft;
    }

    private void CreateCargo()
    {
        UnityEngine.Material cardboard = Material(new Color(0.52f, 0.32f, 0.15f), 0f, 0.12f);
        CargoCube("Box of dishes", new Vector3(-67f, 0.7f, 20f), new Vector3(1.25f, 1.25f, 1.25f), cardboard, 18f, 1, 120, 1.35f);
        CargoCube("Box of books", new Vector3(-60f, 0.55f, 24f), new Vector3(1.4f, 1f, 1.1f), cardboard, 26f, 1, 150, 0.8f);
        CargoCube("Old television", new Vector3(-68f, 0.9f, 25f), new Vector3(1.7f, 1.6f, 1.1f), Material(new Color(0.08f, 0.08f, 0.09f), 0.1f, 0.3f), 32f, 1, 260, 2.2f);

        GameObject sofa = CargoRoot("Heavy sofa — TWO MOVERS", new Vector3(-63f, 0.9f, 16f), new Vector3(3.8f, 1.7f, 1.5f), 125f, 2, 520, 0.7f);
        UnityEngine.Material upholstery = Material(new Color(0.23f, 0.34f, 0.20f), 0f, 0.25f);
        Visual(sofa.transform, "Sofa base", Vector3.zero, new Vector3(3.8f, 1f, 1.5f), upholstery);
        Visual(sofa.transform, "Sofa back", new Vector3(0f, 0.65f, 0.62f), new Vector3(3.8f, 1.2f, 0.28f), upholstery);
        Visual(sofa.transform, "Left arm", new Vector3(-1.75f, 0.3f, 0f), new Vector3(0.3f, 1.2f, 1.5f), upholstery);
        Visual(sofa.transform, "Right arm", new Vector3(1.75f, 0.3f, 0f), new Vector3(0.3f, 1.2f, 1.5f), upholstery);

        GameObject fridge = CargoRoot("Heavy fridge — TWO MOVERS", new Vector3(-56f, 1.15f, 25f), new Vector3(1.45f, 2.3f, 1.35f), 105f, 2, 610, 1.7f);
        Visual(fridge.transform, "Fridge", Vector3.zero, new Vector3(1.45f, 2.3f, 1.35f), Material(new Color(0.77f, 0.77f, 0.70f), 0.12f, 0.32f));

        GameObject chair = CargoRoot("Old chair", new Vector3(-58f, 0.7f, 18f), new Vector3(1.1f, 1.4f, 1.1f), 14f, 1, 90, 1.1f);
        UnityEngine.Material red = Material(new Color(0.40f, 0.10f, 0.08f), 0f, 0.15f);
        Visual(chair.transform, "Seat", Vector3.zero, new Vector3(1.1f, 0.18f, 1.1f), red);
        Visual(chair.transform, "Back", new Vector3(0f, 0.75f, 0.5f), new Vector3(1.1f, 1.5f, 0.16f), red);
    }

    private void CreateVan()
    {
        GameObject van = new GameObject("Moving van");
        van.transform.position = new Vector3(-35f, 0.7f, 4f);
        Rigidbody rb = van.AddComponent<Rigidbody>();
        rb.mass = 1600f;
        rb.linearDamping = 0.45f;
        rb.angularDamping = 2.8f;
        rb.centerOfMass = new Vector3(0f, -0.55f, 0f);
        BoxCollider chassis = van.AddComponent<BoxCollider>();
        chassis.size = new Vector3(5.8f, 1.1f, 2.8f);
        Visual(van.transform, "Cab", new Vector3(1.8f, 0.75f, 0f), new Vector3(2.2f, 2.2f, 2.8f), vanPaint);
        Visual(van.transform, "Cargo floor", new Vector3(-1.25f, 0.7f, 0f), new Vector3(3.8f, 0.18f, 2.6f), floor);
        Visual(van.transform, "Left cargo wall", new Vector3(-1.25f, 1.55f, 1.25f), new Vector3(3.8f, 1.7f, 0.12f), vanPaint);
        Visual(van.transform, "Right cargo wall", new Vector3(-1.25f, 1.55f, -1.25f), new Vector3(3.8f, 1.7f, 0.12f), vanPaint);
        Visual(van.transform, "Roof", new Vector3(-1.25f, 2.38f, 0f), new Vector3(3.8f, 0.12f, 2.6f), vanPaint);
        AddBoxCollider(van, new Vector3(-1.25f, 0.72f, 0f), new Vector3(3.8f, 0.18f, 2.6f));
        AddBoxCollider(van, new Vector3(-1.25f, 1.55f, 1.28f), new Vector3(3.8f, 1.7f, 0.12f));
        AddBoxCollider(van, new Vector3(-1.25f, 1.55f, -1.28f), new Vector3(3.8f, 1.7f, 0.12f));
        van.AddComponent<VanController>();
    }

    private void CreateFirstPersonPlayer()
    {
        GameObject player = new GameObject("First-person mover");
        player.transform.position = new Vector3(-47f, 0.1f, 9f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.85f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 0.93f, 0f);

        UnityEngine.Material uniform = Material(new Color(0.92f, 0.40f, 0.05f), 0f, 0.25f);
        UnityEngine.Material skin = Material(new Color(0.82f, 0.60f, 0.42f), 0f, 0.25f);
        Visual(player.transform, "Stylized long body for other players", new Vector3(0f, 0.95f, 0f), new Vector3(0.58f, 1.25f, 0.42f), uniform, PrimitiveType.Capsule);
        Visual(player.transform, "Head", new Vector3(0f, 1.75f, 0f), Vector3.one * 0.48f, skin, PrimitiveType.Sphere);

        GameObject cameraObject = new GameObject("First-person camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0.08f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 78f;
        camera.nearClipPlane = 0.06f;
        cameraObject.AddComponent<AudioListener>();
        player.AddComponent<PlayerMover>();
    }

    private GameObject CargoCube(string name, Vector3 position, Vector3 size, UnityEngine.Material material, float mass, int movers, int value, float fragility)
    {
        GameObject item = CargoRoot(name, position, size, mass, movers, value, fragility);
        Visual(item.transform, "Visual", Vector3.zero, size, material);
        return item;
    }

    private GameObject CargoRoot(string name, Vector3 position, Vector3 size, float mass, int movers, int value, float fragility)
    {
        GameObject item = new GameObject(name);
        item.transform.position = position;
        item.AddComponent<BoxCollider>().size = size;
        Rigidbody rb = item.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        CarryableItem cargo = item.AddComponent<CarryableItem>();
        cargo.Configure(name, movers, value, fragility);
        return item;
    }

    private GameObject Block(string name, Vector3 position, Vector3 scale, UnityEngine.Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private void Visual(Transform parent, string name, Vector3 position, Vector3 scale, UnityEngine.Material material, PrimitiveType primitive = PrimitiveType.Cube)
    {
        GameObject obj = GameObject.CreatePrimitive(primitive);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(obj.GetComponent<Collider>());
    }

    private static void AddBoxCollider(GameObject target, Vector3 center, Vector3 size)
    {
        BoxCollider collider = target.AddComponent<BoxCollider>();
        collider.center = center;
        collider.size = size;
    }

    private static UnityEngine.Material Material(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        UnityEngine.Material material = new UnityEngine.Material(shader);
        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", smoothness);
        return material;
    }
}
