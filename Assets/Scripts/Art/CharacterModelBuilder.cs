using UnityEngine;

namespace Vendetta.Art
{
    /// <summary>
    /// Art: Luke
    /// Sources / creates 3D warrior character models and authentic Dotanuki Katana weapon
    /// following the Game Design Document:
    /// - Thick & Wide: wide mihaba (blade width), thick kasane (blade thickness)
    /// - Minimalist Design: raw cutting utility, iron tsuba, brass habaki, wrapped tsuka
    /// - Uncolored / prototype blockout aesthetic (God of War prototype style)
    /// - Universal pipeline support (adapts to both URP and Built-in, no magenta pink textures).
    /// </summary>
    public static class CharacterModelBuilder
    {
        public static Material CreateSafeMaterial(Color color, float metallic = 0.2f, float smoothness = 0.3f, bool isEmissive = false)
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

        public static GameObject BuildPlayerWarrior()
        {
            var root = new GameObject("Player_Warrior");

            // Neutral greybox prototype material (God of War style uncolored prototype)
            var protoMat = CreateSafeMaterial(new Color(0.72f, 0.74f, 0.78f), 0.15f, 0.35f);
            var armorPlateMat = CreateSafeMaterial(new Color(0.48f, 0.50f, 0.55f), 0.40f, 0.45f);

            // Torso
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.1f, 0);
            torso.transform.localScale = new Vector3(0.55f, 0.7f, 0.38f);
            torso.GetComponent<Renderer>().material = armorPlateMat;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Head / Helmet
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0, 1.85f, 0);
            head.transform.localScale = new Vector3(0.38f, 0.42f, 0.38f);
            head.GetComponent<Renderer>().material = protoMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Left Shoulder Pauldron (Asymmetrical warrior look like Kratos / Ronin)
            var pauldron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pauldron.name = "Left_Pauldron";
            pauldron.transform.SetParent(root.transform, false);
            pauldron.transform.localPosition = new Vector3(-0.35f, 1.55f, 0);
            pauldron.transform.localRotation = Quaternion.Euler(0, 0, 15f);
            pauldron.transform.localScale = new Vector3(0.26f, 0.22f, 0.38f);
            pauldron.GetComponent<Renderer>().material = armorPlateMat;
            Object.DestroyImmediate(pauldron.GetComponent<Collider>());

            // Sword Mount (Right Hand Grip)
            var swordMount = new GameObject("SwordMount");
            swordMount.transform.SetParent(root.transform, false);
            swordMount.transform.localPosition = new Vector3(0.38f, 1.05f, 0.32f);
            swordMount.transform.localRotation = Quaternion.Euler(0, 30f, -15f);

            // -------------------------------------------------------------
            // AUTHENTIC DOTANUKI KATANA (GDD Specification: Thick & Wide)
            // -------------------------------------------------------------
            BuildDotanukiKatana(swordMount);

            return root;
        }

        private static void BuildDotanukiKatana(GameObject swordMount)
        {
            var bladeSteelMat = CreateSafeMaterial(new Color(0.90f, 0.92f, 0.96f), 0.90f, 0.85f);
            var habakiBrassMat = CreateSafeMaterial(new Color(0.85f, 0.75f, 0.35f), 0.85f, 0.65f);
            var tsubaIronMat = CreateSafeMaterial(new Color(0.22f, 0.22f, 0.25f), 0.70f, 0.40f);
            var tsukaGripMat = CreateSafeMaterial(new Color(0.14f, 0.14f, 0.16f), 0.10f, 0.20f);

            // 1. Tsuka (Handle / Hilt)
            var hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hilt.name = "Katana_Tsuka_Hilt";
            hilt.transform.SetParent(swordMount.transform, false);
            hilt.transform.localPosition = new Vector3(0, -0.22f, 0);
            hilt.transform.localScale = new Vector3(0.045f, 0.20f, 0.045f);
            hilt.GetComponent<Renderer>().material = tsukaGripMat;
            Object.DestroyImmediate(hilt.GetComponent<Collider>());

            // 2. Kashira (Pommel Cap)
            var pommel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pommel.name = "Katana_Kashira";
            pommel.transform.SetParent(swordMount.transform, false);
            pommel.transform.localPosition = new Vector3(0, -0.42f, 0);
            pommel.transform.localScale = new Vector3(0.06f, 0.05f, 0.06f);
            pommel.GetComponent<Renderer>().material = tsubaIronMat;
            Object.DestroyImmediate(pommel.GetComponent<Collider>());

            // 3. Tsuba (Minimalist Oval Disc Guard)
            var tsuba = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tsuba.name = "Katana_Tsuba_Guard";
            tsuba.transform.SetParent(swordMount.transform, false);
            tsuba.transform.localPosition = new Vector3(0, -0.02f, 0);
            tsuba.transform.localScale = new Vector3(0.18f, 0.015f, 0.14f);
            tsuba.GetComponent<Renderer>().material = tsubaIronMat;
            Object.DestroyImmediate(tsuba.GetComponent<Collider>());

            // 4. Habaki (Blade Collar)
            var habaki = GameObject.CreatePrimitive(PrimitiveType.Cube);
            habaki.name = "Katana_Habaki";
            habaki.transform.SetParent(swordMount.transform, false);
            habaki.transform.localPosition = new Vector3(0, 0.04f, 0);
            habaki.transform.localScale = new Vector3(0.055f, 0.07f, 0.045f);
            habaki.GetComponent<Renderer>().material = habakiBrassMat;
            Object.DestroyImmediate(habaki.GetComponent<Collider>());

            // 5. Thick and Wide Dotanuki Blade (Wide Mihaba, Thick Kasane, Curved Kissaki)
            // Segment 1: Lower blade (thick & wide base)
            var bladeLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bladeLower.name = "Greatsword_Blade"; // Retains name for weapon hitbox linkage
            bladeLower.transform.SetParent(swordMount.transform, false);
            bladeLower.transform.localPosition = new Vector3(0, 0.42f, 0);
            bladeLower.transform.localScale = new Vector3(0.095f, 0.70f, 0.042f);
            bladeLower.GetComponent<Renderer>().material = bladeSteelMat;

            // Segment 2: Upper blade with slight curve (Sori)
            var bladeUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bladeUpper.name = "Blade_UpperCurved";
            bladeUpper.transform.SetParent(swordMount.transform, false);
            bladeUpper.transform.localPosition = new Vector3(0.018f, 0.98f, 0);
            bladeUpper.transform.localRotation = Quaternion.Euler(0, 0, -2.5f);
            bladeUpper.transform.localScale = new Vector3(0.088f, 0.48f, 0.038f);
            bladeUpper.GetComponent<Renderer>().material = bladeSteelMat;
            Object.DestroyImmediate(bladeUpper.GetComponent<Collider>());

            // Segment 3: Kissaki (Sharp curved tip)
            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "Blade_Kissaki_Tip";
            tip.transform.SetParent(swordMount.transform, false);
            tip.transform.localPosition = new Vector3(0.042f, 1.25f, 0);
            tip.transform.localRotation = Quaternion.Euler(0, 0, -18f);
            tip.transform.localScale = new Vector3(0.065f, 0.16f, 0.030f);
            tip.GetComponent<Renderer>().material = bladeSteelMat;
            Object.DestroyImmediate(tip.GetComponent<Collider>());
        }

        public static GameObject BuildUncoloredKnight(bool isBoss = false)
        {
            var root = new GameObject(isBoss ? "Boss_TitanKnight" : "Enemy_ArmoredKnight");
            var knightMat = CreateSafeMaterial(isBoss ? new Color(0.55f, 0.20f, 0.20f) : new Color(0.58f, 0.60f, 0.64f), 0.60f, 0.45f);

            // Body
            var torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Knight_Armor_Plate";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.1f, 0);
            torso.transform.localScale = new Vector3(0.7f, 0.9f, 0.45f);
            torso.GetComponent<Renderer>().material = knightMat;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Helmet
            var helmet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            helmet.name = "Knight_Visor_Helmet";
            helmet.transform.SetParent(root.transform, false);
            helmet.transform.localPosition = new Vector3(0, 1.85f, 0);
            helmet.transform.localScale = new Vector3(0.45f, 0.48f, 0.45f);
            helmet.GetComponent<Renderer>().material = knightMat;
            Object.DestroyImmediate(helmet.GetComponent<Collider>());

            // Eye Slit / Visor
            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor_Slit";
            visor.transform.SetParent(helmet.transform, false);
            visor.transform.localPosition = new Vector3(0, 0.05f, 0.52f);
            visor.transform.localScale = new Vector3(0.7f, 0.15f, 0.1f);
            var visorMat = CreateSafeMaterial(isBoss ? Color.red * 2f : new Color(1f, 0.5f, 0.2f), 0.1f, 0.5f, true);
            visor.GetComponent<Renderer>().material = visorMat;
            Object.DestroyImmediate(visor.GetComponent<Collider>());

            // Pauldrons
            for (int side = -1; side <= 1; side += 2)
            {
                var sidePauldron = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sidePauldron.name = side == -1 ? "Left_Pauldron" : "Right_Pauldron";
                sidePauldron.transform.SetParent(root.transform, false);
                sidePauldron.transform.localPosition = new Vector3(side * 0.48f, 1.5f, 0);
                sidePauldron.transform.localScale = new Vector3(0.32f, 0.28f, 0.42f);
                sidePauldron.GetComponent<Renderer>().material = knightMat;
                Object.DestroyImmediate(sidePauldron.GetComponent<Collider>());
            }

            return root;
        }
    }
}
