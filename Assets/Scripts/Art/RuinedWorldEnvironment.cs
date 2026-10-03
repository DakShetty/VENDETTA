using UnityEngine;

namespace Vendetta.Art
{
    /// <summary>
    /// Art: Luke
    /// Sets up a grey box environment, a solid green ground block with depth,
    /// perimeter boundary walls to completely prevent falling or roaming into the void,
    /// and normal asset props that fit a ruined world (crumbling pillars, archways, rubble).
    /// Universal material pipeline support: works seamlessly on both URP and Built-in Render Pipeline without pink textures.
    /// </summary>
    public class RuinedWorldEnvironment : MonoBehaviour
    {
        [Header("Ground Configuration")]
        public Vector2 groundSize = new Vector2(100f, 100f);
        public Color plainGreenGroundColor = new Color(0.24f, 0.38f, 0.22f, 1.0f); // plain green ground

        [Header("Ruined Props")]
        public int numberOfPillars = 16;
        public float arenaRadius = 26f;
        public Color stoneRuinsColor = new Color(0.42f, 0.44f, 0.46f, 1.0f); // greybox stone

        public static Material CreateSafeMaterial(Color color, float metallic = 0.2f, float smoothness = 0.25f, bool isEmissive = false)
        {
            Shader shader = null;
            bool isSRP = (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null) ||
                         (UnityEngine.QualitySettings.renderPipeline != null);

            if (isSRP)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader == null || !shader.isSupported)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null || !shader.isSupported)
            {
                shader = Shader.Find("Diffuse");
            }
            if (shader == null || !shader.isSupported)
            {
                shader = Shader.Find("Mobile/Diffuse");
            }
            if (shader == null || !shader.isSupported)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material mat = new Material(shader);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.color = color;

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (isEmissive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 2.5f);
            }

            return mat;
        }

        private void Start()
        {
            if (transform.childCount == 0)
            {
                BuildEnvironment();
            }
        }

        public void BuildEnvironment()
        {
            // Clear any old child objects to prevent duplicate glitches
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            var groundMat = CreateSafeMaterial(plainGreenGroundColor, 0.05f, 0.15f);
            var ruinsMat = CreateSafeMaterial(stoneRuinsColor, 0.25f, 0.30f);
            var darkStoneMat = CreateSafeMaterial(new Color(0.28f, 0.30f, 0.32f), 0.35f, 0.25f);

            // 1. SOLID 3D GROUND BLOCK WITH DEPTH (Never lets player or enemies slip through into void)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Environment_SolidGround";
            ground.transform.SetParent(transform, false);
            ground.transform.position = new Vector3(0, -2.0f, 0);
            ground.transform.localScale = new Vector3(groundSize.x, 4.0f, groundSize.y);
            ground.GetComponent<Renderer>().material = groundMat;

            // 2. IMPENETRABLE PERIMETER BOUNDARY WALLS (Completely stops entities roaming or falling into void)
            BuildBoundaryWalls(groundSize.x, groundSize.y, darkStoneMat);

            // 3. RUINED WORLD STONE PILLARS
            for (int i = 0; i < numberOfPillars; i++)
            {
                float angle = i * (Mathf.PI * 2f / numberOfPillars);
                float radius = arenaRadius + Random.Range(-2f, 3f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Ruined_Pillar_{i + 1}";
                pillar.transform.SetParent(transform, false);
                float h = Random.Range(4.0f, 8.0f);
                pillar.transform.position = pos + Vector3.up * (h * 0.5f);
                pillar.transform.localScale = new Vector3(Random.Range(1.3f, 2.0f), h * 0.5f, Random.Range(1.3f, 2.0f));
                pillar.transform.rotation = Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(0, 360f), Random.Range(-4f, 4f));
                pillar.GetComponent<Renderer>().material = ruinsMat;

                // Add ruined stone capital block on top
                if (Random.value > 0.35f)
                {
                    var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.name = $"Pillar_Cap_{i + 1}";
                    block.transform.SetParent(pillar.transform, false);
                    block.transform.localPosition = new Vector3(0, 1.05f, 0);
                    block.transform.localScale = new Vector3(1.4f, 0.4f, 1.4f);
                    block.GetComponent<Renderer>().material = ruinsMat;
                }
            }

            // 4. CENTRAL RUINED ASCENSION ALTAR
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            altar.name = "Ruined_Ascension_Altar";
            altar.transform.SetParent(transform, false);
            altar.transform.position = new Vector3(0, 0.3f, 0);
            altar.transform.localScale = new Vector3(7f, 0.6f, 7f);
            altar.GetComponent<Renderer>().material = ruinsMat;

            // Altar center dais
            var dais = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dais.name = "Altar_Center_Dais";
            dais.transform.SetParent(altar.transform, false);
            dais.transform.localPosition = new Vector3(0, 0.6f, 0);
            dais.transform.localScale = new Vector3(0.55f, 0.15f, 0.55f);
            dais.GetComponent<Renderer>().material = darkStoneMat;

            // 5. SCATTERED RUINED STONE SLABS / BLOCKS
            for (int i = 0; i < 8; i++)
            {
                var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = $"Ruined_Slab_{i + 1}";
                slab.transform.SetParent(transform, false);
                float r = Random.Range(10f, 20f);
                float a = i * (Mathf.PI * 2f / 8f) + 0.3f;
                slab.transform.position = new Vector3(Mathf.Cos(a) * r, 0.25f, Mathf.Sin(a) * r);
                slab.transform.localScale = new Vector3(Random.Range(2.2f, 4.0f), Random.Range(0.4f, 0.8f), Random.Range(1.5f, 3.0f));
                slab.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0, 360f), 0);
                slab.GetComponent<Renderer>().material = ruinsMat;
            }

            Debug.Log("<color=green>[ENVIRONMENT]</color> Ruined World 3D Arena constructed with solid depth & boundary walls!");
        }

        private void BuildBoundaryWalls(float width, float depth, Material wallMat)
        {
            float halfW = width * 0.5f;
            float halfD = depth * 0.5f;
            float wallH = 10f;
            float wallThick = 4f;

            CreateWall("BoundaryWall_North", new Vector3(0, wallH * 0.5f, halfD), new Vector3(width + wallThick * 2f, wallH, wallThick), wallMat);
            CreateWall("BoundaryWall_South", new Vector3(0, wallH * 0.5f, -halfD), new Vector3(width + wallThick * 2f, wallH, wallThick), wallMat);
            CreateWall("BoundaryWall_East", new Vector3(halfW, wallH * 0.5f, 0), new Vector3(wallThick, wallH, depth), wallMat);
            CreateWall("BoundaryWall_West", new Vector3(-halfW, wallH * 0.5f, 0), new Vector3(wallThick, wallH, depth), wallMat);
        }

        private void CreateWall(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = mat;
            var col = wall.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = false;
        }
    }
}
