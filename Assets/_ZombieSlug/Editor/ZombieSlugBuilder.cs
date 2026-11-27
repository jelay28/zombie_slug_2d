// -----------------------------------------------------------------------------
// ZombieSlugBuilder.cs - Construcción reproducible del proyecto (menú Zombie Slug).
// Cualquier persona que abra el proyecto en Unity 6.3 LTS y ejecute
// "Zombie Slug > Construir todo" obtiene la misma configuración: capas,
// etiquetas, matriz de colisiones, importación, datos de balance, animaciones,
// prefabs con variantes y las tres escenas en la lista de construcción.
// Sirve como prueba de reproducibilidad (sección j del informe).
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace ZombieSlug.EditorTools
{
    public static class ZombieSlugBuilder
    {
        const string R = "Assets/_ZombieSlug/";
        const string ART = R + "Art/";
        static readonly string[] CAPAS = { "Jugador", "Enemigo", "EnemigoGolpe", "Proyectil", "Suelo", "Plataforma", "Hacha", "Tablero" };
        static readonly string[] ORDEN = { "Fondo", "Escenario", "Personajes", "Proyectiles", "PrimerPlano" };
        const float CAM_Y = 4.6875f;     // 11,25 m de alto visibles: suelo en y = 0 con 0,94 m de banda inferior
        const float CAM_SIZE = 5.625f;   // 360 px / 32 px por unidad / 2

        // ======================================================================
        [MenuItem("Zombie Slug/Construir todo", false, 0)]
        public static void ConstruirTodo()
        {
            Configurar();
            ReimportarArte();
            CrearDatos();
            CrearAnimaciones();
            CrearPrefabs();
            ConstruirEscenas();
            Debug.Log("[Zombie Slug] Proyecto construido: 3 escenas, 7 prefabs, 13 datos de balance.");
        }

        // ---------------------------------------------------------------- 1
        [MenuItem("Zombie Slug/1. Configurar proyecto", false, 20)]
        public static void Configurar()
        {
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Etiquetas
            var tags = tm.FindProperty("tags");
            foreach (string t in new[] { "Enemigo", "Tablero" })
            {
                bool existe = false;
                for (int i = 0; i < tags.arraySize; i++) if (tags.GetArrayElementAtIndex(i).stringValue == t) existe = true;
                if (!existe) { tags.InsertArrayElementAtIndex(tags.arraySize); tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = t; }
            }
            // Capas de física: 6 a 13
            var layers = tm.FindProperty("layers");
            for (int i = 0; i < CAPAS.Length; i++) layers.GetArrayElementAtIndex(6 + i).stringValue = CAPAS[i];
            // Capas de ordenamiento
            var sl = tm.FindProperty("m_SortingLayers");
            foreach (string n in ORDEN)
            {
                bool existe = false;
                for (int i = 0; i < sl.arraySize; i++) if (sl.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == n) existe = true;
                if (existe) continue;
                sl.InsertArrayElementAtIndex(sl.arraySize);
                var e = sl.GetArrayElementAtIndex(sl.arraySize - 1);
                e.FindPropertyRelative("name").stringValue = n;
                e.FindPropertyRelative("uniqueID").intValue = (int)(Mathf.Abs(n.GetHashCode()) % 100000000) + 1000;
            }
            tm.ApplyModifiedPropertiesWithoutUndo();

            // Matriz de colisiones 2D: todo apagado entre capas propias y luego solo los pares necesarios
            int[] idx = new int[CAPAS.Length];
            for (int i = 0; i < CAPAS.Length; i++) idx[i] = 6 + i;
            for (int a = 0; a < 32; a++)
                foreach (int b in idx) Physics2D.IgnoreLayerCollision(a, b, true);
            Par("Jugador", "Suelo"); Par("Jugador", "Plataforma"); Par("Jugador", "EnemigoGolpe"); Par("Jugador", "Tablero");
            Par("Enemigo", "Suelo"); Par("Enemigo", "Plataforma"); Par("Enemigo", "Proyectil"); Par("Enemigo", "Hacha");
            Par("Proyectil", "Suelo");

            // Reproductor y presentación
            PlayerSettings.companyName = "Jelay";
            PlayerSettings.productName = "Zombie Slug";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            Time.fixedDeltaTime = 0.02f;

            foreach (string c in new[] { "Animations", "Prefabs", "Scenes", "ScriptableObjects/Armas", "ScriptableObjects/Oleadas", "Art/Tiles" })
                Directory.CreateDirectory(R + c);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Zombie Slug] 1/6 Configuración aplicada (etiquetas, 8 capas de física, 5 capas de orden, matriz 2D, 1920x1080).");
        }

        static void Par(string a, string b) { Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(a), LayerMask.NameToLayer(b), false); }

        // ---------------------------------------------------------------- 2
        [MenuItem("Zombie Slug/2. Reimportar arte (PPU 32, Point)", false, 21)]
        public static void ReimportarArte()
        {
            int n = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ART.TrimEnd('/') }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(p) as TextureImporter;
                if (ti == null) continue;
                ZombieSlugImportador.Aplicar(ti, p);
                ti.SaveAndReimport();
                n++;
            }
            Debug.Log("[Zombie Slug] 2/6 Arte reimportado con criterio único: " + n + " texturas.");
        }

        // ---------------------------------------------------------------- 3
        [MenuItem("Zombie Slug/3. Crear datos de balance", false, 22)]
        public static void CrearDatos()
        {
            Arma("arm_pistola", "ARM-01", "Pistola", 1f, 3f, 1, 0f, 12f, 20f, -1, 0f, 0);
            Arma("arm_escopeta", "ARM-02", "Escopeta", 1f, 1.2f, 5, 30f, 12f, 6f, 24, 1.5f, 50);
            Arma("arm_ametralladora", "ARM-03", "Ametralladora", 1f, 8f, 1, 12f, 12f, 20f, 240, 0f, 80);
            int[,] comp = { { 6, 0, 0 }, { 10, 0, 0 }, { 8, 3, 0 }, { 14, 4, 0 }, { 16, 8, 0 }, { 12, 4, 1 }, { 16, 8, 2 }, { 18, 10, 3 }, { 20, 12, 4 }, { 6, 4, 0 } };
            for (int n = 1; n <= 10; n++)
            {
                string p = R + "ScriptableObjects/Oleadas/ola_" + n.ToString("00") + ".asset";
                var o = Cargar<OleadaData>(p);
                o.numero = n;
                o.caminantes = comp[n - 1, 0]; o.corredores = comp[n - 1, 1]; o.tanques = comp[n - 1, 2];
                o.jefe = n == 10;
                o.intervalo = Mathf.Max(1.2f, 2.4f - 0.13f * (n - 1));
                o.multiplicadorVida = 1f + 0.05f * (n - 1);
                o.arena = n <= 3 ? 0 : (n <= 6 ? 1 : 2);
                EditorUtility.SetDirty(o);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Zombie Slug] 3/6 Datos creados: 3 armas y 10 oleadas.");
        }

        static void Arma(string archivo, string id, string nombre, float dano, float cad, int perd, float disp, float vel, float alc, int mun, float emp, int costo)
        {
            var a = Cargar<ArmaData>(R + "ScriptableObjects/Armas/" + archivo + ".asset");
            a.id = id; a.nombre = nombre; a.dano = dano; a.cadencia = cad; a.perdigones = perd; a.dispersion = disp;
            a.velocidadProyectil = vel; a.alcance = alc; a.municionPorOleada = mun; a.empuje = emp; a.costo = costo;
            EditorUtility.SetDirty(a);
        }

        static T Cargar<T>(string ruta) where T : ScriptableObject
        {
            var x = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (x == null) { x = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(x, ruta); }
            return x;
        }

        // ---------------------------------------------------------------- 4
        [MenuItem("Zombie Slug/4. Crear animaciones", false, 23)]
        public static void CrearAnimaciones()
        {
            string J = ART + "Sprites/Jugador/spr_jugador_ken_";
            string E = ART + "Sprites/Enemigos/spr_enemigo_";
            string A = R + "Animations/";
            var reposo = Clip(A + "anim_ken_reposo.anim", 4f, true, J + "idle_01", J + "idle_02");
            var correr = Clip(A + "anim_ken_correr.anim", 10f, true, J + "correr_01", J + "correr_02", J + "correr_03", J + "correr_04");
            var saltar = Clip(A + "anim_ken_saltar.anim", 1f, false, J + "saltar_01");
            var disparar = Clip(A + "anim_ken_disparar.anim", 1f, false, J + "disparar_01");
            var dano = Clip(A + "anim_ken_dano.anim", 1f, false, J + "dano_01");
            var muerte = Clip(A + "anim_ken_muerte.anim", 1f, false, J + "muerte_01");

            // Máquina de estados de Ken
            string pk = A + "ac_ken.controller";
            AssetDatabase.DeleteAsset(pk);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(pk);
            ac.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
            ac.AddParameter("EnSuelo", AnimatorControllerParameterType.Bool);
            ac.AddParameter("Disparar", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Dano", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Morir", AnimatorControllerParameterType.Trigger);
            var sm = ac.layers[0].stateMachine;
            var sReposo = sm.AddState("Reposo", new Vector3(300, 0, 0)); sReposo.motion = reposo;
            var sCorrer = sm.AddState("Correr", new Vector3(300, 120, 0)); sCorrer.motion = correr;
            var sSaltar = sm.AddState("Saltar", new Vector3(560, 60, 0)); sSaltar.motion = saltar;
            var sDisp = sm.AddState("Disparar", new Vector3(40, 120, 0)); sDisp.motion = disparar;
            var sDano = sm.AddState("Dano", new Vector3(560, 200, 0)); sDano.motion = dano;
            var sMuerte = sm.AddState("Muerte", new Vector3(560, -80, 0)); sMuerte.motion = muerte;
            sm.defaultState = sReposo;
            Tr(sReposo, sCorrer, "Velocidad", AnimatorConditionMode.Greater, 0.1f);
            Tr(sCorrer, sReposo, "Velocidad", AnimatorConditionMode.Less, 0.1f);
            var t1 = sm.AddAnyStateTransition(sSaltar); t1.AddCondition(AnimatorConditionMode.IfNot, 0, "EnSuelo"); t1.duration = 0; t1.canTransitionToSelf = false;
            var t2 = sSaltar.AddTransition(sReposo); t2.AddCondition(AnimatorConditionMode.If, 0, "EnSuelo"); t2.duration = 0; t2.hasExitTime = false;
            var t3 = sm.AddAnyStateTransition(sDisp); t3.AddCondition(AnimatorConditionMode.If, 0, "Disparar"); t3.duration = 0;
            var t4 = sDisp.AddTransition(sReposo); t4.hasExitTime = true; t4.exitTime = 1f; t4.duration = 0;
            var t5 = sm.AddAnyStateTransition(sDano); t5.AddCondition(AnimatorConditionMode.If, 0, "Dano"); t5.duration = 0;
            var t6 = sDano.AddTransition(sReposo); t6.hasExitTime = true; t6.exitTime = 1f; t6.duration = 0;
            var t7 = sm.AddAnyStateTransition(sMuerte); t7.AddCondition(AnimatorConditionMode.If, 0, "Morir"); t7.duration = 0;

            // Controlador base de enemigos + override controllers (reutilización)
            var cam = Clip(A + "anim_caminante_caminar.anim", 4f, true, E + "caminante_caminar_01", E + "caminante_caminar_02");
            var cor = Clip(A + "anim_corredor_correr.anim", 8f, true, E + "corredor_correr_01", E + "corredor_correr_02");
            var tan = Clip(A + "anim_tanque_caminar.anim", 3f, true, E + "tanque_caminar_01", E + "tanque_caminar_02");
            var jef = Clip(A + "anim_jefe_caminar.anim", 3f, true, E + "jefe_caminar_01", E + "jefe_caminar_02");
            string pe = A + "ac_enemigo_base.controller";
            AssetDatabase.DeleteAsset(pe);
            var ae = AnimatorController.CreateAnimatorControllerAtPath(pe);
            var se = ae.layers[0].stateMachine.AddState("Caminar"); se.motion = cam;
            Override(A + "aoc_corredor.overrideController", ae, cam, cor);
            Override(A + "aoc_tanque.overrideController", ae, cam, tan);
            Override(A + "aoc_jefe.overrideController", ae, cam, jef);
            AssetDatabase.SaveAssets();
            Debug.Log("[Zombie Slug] 4/6 Animaciones: 10 clips, 2 controladores y 3 override controllers.");
        }

        static void Tr(AnimatorState a, AnimatorState b, string p, AnimatorConditionMode m, float v)
        {
            var t = a.AddTransition(b); t.AddCondition(m, v, p); t.hasExitTime = false; t.duration = 0;
        }

        static AnimationClip Clip(string ruta, float fps, bool bucle, params string[] sprites)
        {
            AssetDatabase.DeleteAsset(ruta);
            var clip = new AnimationClip { frameRate = fps };
            var b = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i <= sprites.Length; i++)
            {
                keys[i].time = i / fps;
                keys[i].value = Spr(sprites[Mathf.Min(i, sprites.Length - 1)]);
            }
            AnimationUtility.SetObjectReferenceCurve(clip, b, keys);
            var s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = bucle;
            AnimationUtility.SetAnimationClipSettings(clip, s);
            AssetDatabase.CreateAsset(clip, ruta);
            return clip;
        }

        static void Override(string ruta, AnimatorController baseCtrl, AnimationClip original, AnimationClip nuevo)
        {
            AssetDatabase.DeleteAsset(ruta);
            var o = new AnimatorOverrideController(baseCtrl);
            o[original] = nuevo;
            AssetDatabase.CreateAsset(o, ruta);
        }

        static Sprite Spr(string rutaSinExt) { return AssetDatabase.LoadAssetAtPath<Sprite>(rutaSinExt + ".png"); }

        // ---------------------------------------------------------------- 5
        [MenuItem("Zombie Slug/5. Crear prefabs", false, 24)]
        public static void CrearPrefabs()
        {
            string P = R + "Prefabs/";
            string A = R + "Animations/";
            var sinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(P + "pm_sin_friccion.physicsMaterial2D");
            if (sinFriccion == null) { sinFriccion = new PhysicsMaterial2D("pm_sin_friccion") { friction = 0f, bounciness = 0f }; AssetDatabase.CreateAsset(sinFriccion, P + "pm_sin_friccion.physicsMaterial2D"); }

            // ---- Ken (JUG-01)
            var ken = new GameObject("Ken");
            ken.tag = "Player"; ken.layer = L("Jugador");
            Cuerpo(ken, true);
            var cap = ken.AddComponent<CapsuleCollider2D>();
            cap.size = new Vector2(0.7f, 1.7f); cap.offset = new Vector2(0f, 0.86f); cap.sharedMaterial = sinFriccion;
            var dib = Dibujo(ken, ART + "Sprites/Jugador/spr_jugador_ken_idle_01", "Personajes", 0);
            var anim = dib.gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(A + "ac_ken.controller");
            var pies = new GameObject("Pies").transform; pies.SetParent(ken.transform, false); pies.localPosition = Vector3.zero;
            var boca = new GameObject("BocaArma").transform; boca.SetParent(ken.transform, false); boca.localPosition = new Vector3(0.6f, 1.05f, 0f);
            var zona = new GameObject("ZonaHacha"); zona.layer = L("Hacha"); zona.transform.SetParent(ken.transform, false);
            var cz = zona.AddComponent<CircleCollider2D>(); cz.isTrigger = true; cz.radius = 1f; cz.offset = new Vector2(0f, 0.9f);
            zona.AddComponent<MeleeAuto>();
            var pc = ken.AddComponent<PlayerController>();
            Set(pc, "pies", pies); Set(pc, "animador", anim);
            SetInt(pc, "capasSuelo", Mascara("Suelo", "Plataforma")); SetInt(pc, "capaPlataforma", Mascara("Plataforma"));
            var ps = ken.AddComponent<PlayerShooting>();
            Set(ps, "armaInicial", AssetDatabase.LoadAssetAtPath<ArmaData>(R + "ScriptableObjects/Armas/arm_pistola.asset"));
            Set(ps, "bocaDelArma", boca); Set(ps, "control", pc);
            var ph = ken.AddComponent<PlayerHealth>(); Set(ph, "dibujo", dib);
            Guardar(ken, P + "pfb_jugador_ken.prefab");

            // ---- Enemigo base (estructura común; las variantes agregan el comportamiento)
            var eb = new GameObject("Enemigo");
            eb.tag = "Enemigo"; eb.layer = L("Enemigo");
            Cuerpo(eb, true);
            var ec = eb.AddComponent<CapsuleCollider2D>(); ec.size = new Vector2(0.75f, 1.7f); ec.offset = new Vector2(0f, 0.86f); ec.sharedMaterial = sinFriccion;
            var ed = Dibujo(eb, ART + "Sprites/Enemigos/spr_enemigo_caminante_caminar_01", "Personajes", 1);
            var ea = ed.gameObject.AddComponent<Animator>();
            ea.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(A + "ac_enemigo_base.controller");
            var golpe = new GameObject("Golpe"); golpe.layer = L("EnemigoGolpe"); golpe.transform.SetParent(eb.transform, false);
            var gb = golpe.AddComponent<BoxCollider2D>(); gb.isTrigger = true; gb.size = new Vector2(0.9f, 1.6f); gb.offset = new Vector2(0f, 0.85f);
            var basePrefab = Guardar(eb, P + "pfb_enemigo_base.prefab");

            Variante(basePrefab, P + "pfb_enemigo_caminante.prefab", typeof(EnemyBase), 3f, 1.2f, 10, 10, 0f, null, null, 1f);
            Variante(basePrefab, P + "pfb_enemigo_corredor.prefab", typeof(EnemyRunner), 2f, 2.4f, 8, 20, 0f, ART + "Sprites/Enemigos/spr_enemigo_corredor_correr_01", A + "aoc_corredor.overrideController", 1f);
            Variante(basePrefab, P + "pfb_enemigo_tanque.prefab", typeof(EnemyBase), 8f, 0.8f, 25, 50, 0.7f, ART + "Sprites/Enemigos/spr_enemigo_tanque_caminar_01", A + "aoc_tanque.overrideController", 1.45f);
            Variante(basePrefab, P + "pfb_enemigo_jefe.prefab", typeof(BossController), 60f, 1.0f, 30, 150, 0.9f, ART + "Sprites/Enemigos/spr_enemigo_jefe_caminar_01", A + "aoc_jefe.overrideController", 2.3f);

            // ---- Proyectil
            var pr = new GameObject("Proyectil"); pr.layer = L("Proyectil");
            var prb = pr.AddComponent<Rigidbody2D>(); prb.gravityScale = 0f; prb.constraints = RigidbodyConstraints2D.FreezeRotation; prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var pcol = pr.AddComponent<BoxCollider2D>(); pcol.isTrigger = true; pcol.size = new Vector2(0.25f, 0.125f);
            var psr = pr.AddComponent<SpriteRenderer>(); psr.sprite = Spr(ART + "Sprites/Proyectiles/spr_proyectil_bala"); psr.sortingLayerName = "Proyectiles";
            pr.AddComponent<Projectile>();
            Guardar(pr, P + "pfb_proyectil.prefab");

            // ---- Tablero de Don Kuro
            var tb = new GameObject("Tablero"); tb.tag = "Tablero"; tb.layer = L("Tablero");
            var tsr = tb.AddComponent<SpriteRenderer>(); tsr.sprite = Spr(ART + "Sprites/Escenario/spr_escenario_tablero_01"); tsr.sortingLayerName = "Escenario";
            var luz = new GameObject("Luz"); luz.transform.SetParent(tb.transform, false); luz.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            var lsr = luz.AddComponent<SpriteRenderer>(); lsr.sprite = Spr(ART + "Sprites/Escenario/spr_escenario_tablero_luz"); lsr.sortingLayerName = "Escenario"; lsr.sortingOrder = 1; lsr.enabled = false;
            var tc = tb.AddComponent<BoxCollider2D>(); tc.isTrigger = true; tc.size = new Vector2(2.6f, 2f); tc.offset = new Vector2(0f, 1f); tc.enabled = false;
            var rbd = tb.AddComponent<RewardBoard>(); Set(rbd, "zona", tc); Set(rbd, "luz", lsr); rbd.enabled = false;
            Guardar(tb, P + "pfb_escenario_tablero.prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("[Zombie Slug] 5/6 Prefabs: Ken, enemigo base + 4 variantes, proyectil y tablero.");
        }

        static Rigidbody2D Cuerpo(GameObject go, bool dinamico)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = dinamico ? RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            return rb;
        }

        static SpriteRenderer Dibujo(GameObject padre, string sprite, string capa, int orden)
        {
            var d = new GameObject("Dibujo"); d.transform.SetParent(padre.transform, false);
            var sr = d.AddComponent<SpriteRenderer>(); sr.sprite = Spr(sprite); sr.sortingLayerName = capa; sr.sortingOrder = orden;
            return sr;
        }

        static void Variante(GameObject basePrefab, string ruta, System.Type comportamiento, float vida, float vel, int dano, int pts, float resist, string sprite, string controlador, float escala)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            var e = (EnemyBase)inst.AddComponent(comportamiento);
            SetF(e, "vidaBase", vida); SetF(e, "velocidad", vel); SetInt(e, "danoContacto", dano); SetInt(e, "puntos", pts); SetF(e, "resistenciaEmpuje", resist);
            var dib = inst.transform.Find("Dibujo").GetComponent<SpriteRenderer>();
            Set(e, "dibujo", dib);
            if (sprite != null) dib.sprite = Spr(sprite);
            if (controlador != null) dib.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controlador);
            if (e is EnemyRunner) SetInt(e, "capasSuelo", Mascara("Suelo", "Plataforma"));
            if (escala != 1f)
            {
                var c = inst.GetComponent<CapsuleCollider2D>(); c.size = new Vector2(0.75f * escala, 1.7f * escala); c.offset = new Vector2(0f, 0.86f * escala);
                var g = inst.transform.Find("Golpe").GetComponent<BoxCollider2D>(); g.size = new Vector2(0.9f * escala, 1.6f * escala); g.offset = new Vector2(0f, 0.85f * escala);
            }
            inst.name = Path.GetFileNameWithoutExtension(ruta);
            PrefabUtility.SaveAsPrefabAsset(inst, ruta);   // al guardar una instancia de prefab se crea una VARIANTE
            Object.DestroyImmediate(inst);
        }

        static GameObject Guardar(GameObject go, string ruta)
        {
            var p = PrefabUtility.SaveAsPrefabAsset(go, ruta);
            Object.DestroyImmediate(go);
            return p;
        }

        // ---------------------------------------------------------------- 6
        [MenuItem("Zombie Slug/6. Construir escenas", false, 25)]
        public static void ConstruirEscenas()
        {
            string S = R + "Scenes/";
            EscenaMenu(S + "Menu.unity");
            EscenaResultado(S + "Resultado.unity");
            EscenaJuego(S + "Juego.unity");
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(S + "Menu.unity", true),
                new EditorBuildSettingsScene(S + "Juego.unity", true),
                new EditorBuildSettingsScene(S + "Resultado.unity", true) };
            EditorSceneManager.OpenScene(S + "Juego.unity");
            Debug.Log("[Zombie Slug] 6/6 Escenas construidas y agregadas a la lista de construcción (Menu, Juego, Resultado).");
        }

        static void EscenaJuego(string ruta)
        {
            var esc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string P = R + "Prefabs/";

            // --- Sistemas
            var sistemas = Vacio("--- Sistemas", null);
            var sesion = Vacio("GameSession", null); sesion.AddComponent<GameSession>();   // raíz: DontDestroyOnLoad solo funciona en objetos raíz
            var gmGo = Vacio("GameManager", sistemas.transform);
            var gm = gmGo.AddComponent<GameManager>();
            var score = gmGo.AddComponent<ScoreManager>();
            var shop = gmGo.AddComponent<ShopController>();
            var wmGo = Vacio("WaveManager", sistemas.transform);
            var wm = wmGo.AddComponent<WaveManager>();
            var piscinas = Vacio("Piscinas", sistemas.transform);
            var pCam = Piscina("Piscina_Caminantes", piscinas.transform, P + "pfb_enemigo_caminante.prefab", 20);
            var pCor = Piscina("Piscina_Corredores", piscinas.transform, P + "pfb_enemigo_corredor.prefab", 12);
            var pTan = Piscina("Piscina_Tanques", piscinas.transform, P + "pfb_enemigo_tanque.prefab", 4);
            var pJef = Piscina("Piscina_Jefe", piscinas.transform, P + "pfb_enemigo_jefe.prefab", 1);
            var pPro = Piscina("Piscina_Proyectiles", piscinas.transform, P + "pfb_proyectil.prefab", 16);

            // --- Cámara (ortográfica, 11,25 x 20 m) y luz global 2D
            var camGo = Vacio("Main Camera", null); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = CAM_SIZE; cam.transform.position = new Vector3(0f, CAM_Y, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.13f, 0.34f, 0.38f);
            camGo.AddComponent<AudioListener>();
            AgregarPixelPerfect(camGo);
            AgregarLuzGlobal();

            // --- Escenario
            var escenario = Vacio("--- Escenario", null);
            var arenasGo = Vacio("Arenas", escenario.transform);
            var arenaMgr = arenasGo.AddComponent<ArenaManager>();
            var arenas = new GameObject[3];
            string[] nombres = { "Arena1_Calle", "Arena2_MetroElevado", "Arena3_Cisternas" };
            for (int i = 0; i < 3; i++)
            {
                arenas[i] = Vacio(nombres[i], arenasGo.transform);
                string[] capas = { "lejana", "media", "cercana" };
                for (int c = 0; c < 3; c++)
                {
                    var f = Vacio("Fondo_" + capas[c], arenas[i].transform);
                    f.transform.position = new Vector3(0f, CAM_Y, 0f);
                    var sr = f.AddComponent<SpriteRenderer>();
                    sr.sprite = Spr(ART + "Fondos/Arena" + (i + 1) + "/bg_arena" + (i + 1) + "_capa_" + capas[c]);
                    sr.sortingLayerName = "Fondo"; sr.sortingOrder = c;
                }
                arenas[i].SetActive(i == 0);   // estado inicial guardado en la escena
            }
            SetArr(arenaMgr, "arenas", arenas);
            ConstruirTilemaps(escenario.transform);
            var limites = Vacio("Limites", escenario.transform);
            Muro("Muro_Izquierdo", limites.transform, -10.5f);
            Muro("Muro_Derecho", limites.transform, 10.5f);
            var puntos = Vacio("PuntosAparicion", escenario.transform);
            var s1 = Vacio("S1_Izquierda", puntos.transform); s1.transform.position = new Vector3(-9.3f, 0.1f, 0f);
            var s2 = Vacio("S2_Derecha", puntos.transform); s2.transform.position = new Vector3(9.3f, 0.1f, 0f);
            var s3 = Vacio("S3_Vagon", puntos.transform); s3.transform.position = new Vector3(0f, 9.5f, 0f);
            var tablero = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(P + "pfb_escenario_tablero.prefab"));
            tablero.transform.SetParent(escenario.transform); tablero.transform.position = new Vector3(8.4f, 0f, 0f);

            // --- Jugador
            var jugGrupo = Vacio("--- Jugador", null);
            var ken = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(P + "pfb_jugador_ken.prefab"));
            ken.transform.SetParent(jugGrupo.transform); ken.transform.position = new Vector3(0f, 0.05f, 0f);
            Set(ken.GetComponent<PlayerShooting>(), "piscinaProyectiles", pPro);

            // --- Interfaz
            ConstruirInterfaz(shop);

            // --- Referencias entre sistemas (campos serializados, visibles en el inspector)
            Set(gm, "oleadas", wm); Set(gm, "tablero", tablero.GetComponent<RewardBoard>());
            Set(gm, "controlJugador", ken.GetComponent<PlayerController>()); Set(gm, "disparoJugador", ken.GetComponent<PlayerShooting>());
            Set(gm, "puntaje", score);
            Set(shop, "puntaje", score); Set(shop, "salud", ken.GetComponent<PlayerHealth>()); Set(shop, "disparo", ken.GetComponent<PlayerShooting>());
            Set(shop, "escopeta", AssetDatabase.LoadAssetAtPath<ArmaData>(R + "ScriptableObjects/Armas/arm_escopeta.asset"));
            Set(shop, "ametralladora", AssetDatabase.LoadAssetAtPath<ArmaData>(R + "ScriptableObjects/Armas/arm_ametralladora.asset"));
            var olas = new Object[10];
            for (int n = 1; n <= 10; n++) olas[n - 1] = AssetDatabase.LoadAssetAtPath<OleadaData>(R + "ScriptableObjects/Oleadas/ola_" + n.ToString("00") + ".asset");
            SetArr(wm, "oleadas", olas);
            SetArr(wm, "puntosAparicion", new Object[] { s1.transform, s2.transform, s3.transform });
            Set(wm, "jugador", ken.transform);
            Set(wm, "piscinaCaminantes", pCam); Set(wm, "piscinaCorredores", pCor); Set(wm, "piscinaTanques", pTan); Set(wm, "piscinaJefe", pJef);
            Set(wm, "arenas", arenaMgr);

            EditorSceneManager.SaveScene(esc, ruta);
        }

        static void ConstruirTilemaps(Transform padre)
        {
            var tSuelo = Tile(R + "Art/Tiles/tile_comun_suelo_01.asset", ART + "Tiles/tile_comun_suelo_01");
            var tPlat = Tile(R + "Art/Tiles/tile_comun_plataforma_01.asset", ART + "Tiles/tile_comun_plataforma_01");
            var grid = Vacio("Grid", padre); grid.AddComponent<Grid>();

            // Tilemaps: solo dibujo (las colisiones van en primitivas, más baratas y predecibles)
            var suelo = CapaTiles("Tilemap_Suelo", grid.transform);
            for (int x = -10; x < 10; x++) suelo.SetTile(new Vector3Int(x, -1, 0), tSuelo);
            var plat = CapaTiles("Tilemap_Plataformas", grid.transform);
            for (int x = -8; x <= -5; x++) plat.SetTile(new Vector3Int(x, 2, 0), tPlat);   // 2a: superficie en y = 3 m
            for (int x = 4; x <= 7; x++) plat.SetTile(new Vector3Int(x, 2, 0), tPlat);     // 2b: superficie en y = 3 m
            for (int x = -3; x <= 2; x++) plat.SetTile(new Vector3Int(x, 5, 0), tPlat);    // 3:  superficie en y = 6 m

            // Colisiones: suelo continuo (20 x 1 m) y tres plataformas de una vía (PlatformEffector2D)
            var col = Vacio("Colisiones", padre);
            var cs = Vacio("Colision_Suelo", col.transform); cs.layer = L("Suelo"); cs.transform.position = new Vector3(0f, -0.5f, 0f);
            cs.AddComponent<BoxCollider2D>().size = new Vector2(20f, 1f);
            Plataforma("Plataforma_2a", col.transform, new Vector2(-6f, 2.85f), 4f);
            Plataforma("Plataforma_2b", col.transform, new Vector2(6f, 2.85f), 4f);
            Plataforma("Plataforma_3", col.transform, new Vector2(0f, 5.85f), 6f);
        }

        static void Plataforma(string n, Transform padre, Vector2 pos, float ancho)
        {
            var p = Vacio(n, padre); p.layer = L("Plataforma"); p.transform.position = pos;
            var b = p.AddComponent<BoxCollider2D>(); b.size = new Vector2(ancho, 0.3f); b.usedByEffector = true;
            var ef = p.AddComponent<PlatformEffector2D>(); ef.useOneWay = true; ef.surfaceArc = 160f;
        }

        static Tilemap CapaTiles(string nombre, Transform grid)
        {
            var go = Vacio(nombre, grid);
            var tm = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>(); tr.sortingLayerName = "Escenario";
            return tm;
        }

        static Tile Tile(string ruta, string sprite)
        {
            var t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
            if (t == null) { t = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(t, ruta); }
            t.sprite = Spr(sprite); t.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
            EditorUtility.SetDirty(t);
            return t;
        }

        static void Muro(string n, Transform padre, float x)
        {
            var m = Vacio(n, padre); m.layer = L("Suelo"); m.transform.position = new Vector3(x, 5f, 0f);
            var b = m.AddComponent<BoxCollider2D>(); b.size = new Vector2(1f, 20f);
        }

        static ObjectPool Piscina(string n, Transform padre, string prefab, int tam)
        {
            var go = Vacio(n, padre);
            var p = go.AddComponent<ObjectPool>();
            Set(p, "prefab", AssetDatabase.LoadAssetAtPath<GameObject>(prefab)); SetInt(p, "tamano", tam);
            return p;
        }

        // ---------------------------------------------------------------- Interfaz
        static Font fuente;
        static Font Fuente() { if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return fuente; }

        static UIManager ConstruirInterfaz(ShopController shop)
        {
            var grupo = Vacio("--- Interfaz", null);
            var lienzo = Lienzo(grupo.transform);
            var uim = lienzo.AddComponent<UIManager>();
            var hud = lienzo.AddComponent<HUDController>();
            Sistema(grupo.transform);

            // HUD
            var pHUD = Panel("Panel_HUD", lienzo.transform, new Color(0, 0, 0, 0));
            var fondoBarra = Img("Barra_Fondo", pHUD.transform, new Color(0.1f, 0.1f, 0.1f, 0.8f), new Vector2(0, 1), new Vector2(30, -30), new Vector2(420, 36));
            var relleno = Img("Barra_Relleno", fondoBarra.transform, new Color(0.85f, 0.15f, 0.15f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(420, 36));
            relleno.rectTransform.pivot = new Vector2(0, 0.5f);
            relleno.sprite = Spr(ART + "Sprites/UI/spr_ui_blanco"); relleno.type = Image.Type.Filled; relleno.fillMethod = Image.FillMethod.Horizontal; relleno.fillAmount = 1f;
            var tSalud = Txt("Texto_Salud", fondoBarra.transform, "100 / 100", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 36));
            var tVidas = Txt("Texto_Vidas", pHUD.transform, "Vidas extra: 1", 26, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -76), new Vector2(420, 40));
            var tOla = Txt("Texto_Oleada", pHUD.transform, "Oleada 0 / 10", 34, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(500, 50));
            var tPts = Txt("Texto_Puntos", pHUD.transform, "Puntos: 0", 30, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-30, -26), new Vector2(500, 44));
            var tMon = Txt("Texto_Monedas", pHUD.transform, "Monedas: 0", 30, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-30, -70), new Vector2(500, 44));
            var tArma = Txt("Texto_Arma", pHUD.transform, "Pistola  ∞", 30, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(30, 26), new Vector2(600, 44));
            Set(hud, "barraSalud", relleno); Set(hud, "textoSalud", tSalud); Set(hud, "textoVidas", tVidas); Set(hud, "textoOleada", tOla);
            Set(hud, "textoPuntos", tPts); Set(hud, "textoMonedas", tMon); Set(hud, "textoArma", tArma);

            var aviso = Txt("Texto_Aviso", lienzo.transform, "OLEADA 1", 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(1600, 120));
            aviso.gameObject.AddComponent<Outline>().effectDistance = new Vector2(3, -3);
            aviso.gameObject.SetActive(false);
            var ayuda = Txt("Ayuda_Tablero", lienzo.transform, "E: abrir el tablero de Don Kuro", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(900, 50));
            ayuda.gameObject.AddComponent<Outline>();
            ayuda.gameObject.SetActive(false);

            // Tienda
            var pT = Ventana("Panel_Tienda", lienzo.transform, new Vector2(960, 600));
            Txt("Titulo", pT.transform, "TABLERO DE DON KURO", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(900, 60));
            Boton("Boton_Salud", pT.transform, "Salud +25 %   (30)", new Vector2(-230, 110), uim.BotonSalud);
            Boton("Boton_Vida", pT.transform, "Vida extra   (40)", new Vector2(230, 110), uim.BotonVida);
            Boton("Boton_Escopeta", pT.transform, "Escopeta   (50)", new Vector2(-230, 0), uim.BotonEscopeta);
            Boton("Boton_Ametralladora", pT.transform, "Ametralladora   (80)", new Vector2(230, 0), uim.BotonAmetralladora);
            var msg = Txt("Texto_Mensaje", pT.transform, "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(900, 50));
            Boton("Boton_Continuar", pT.transform, "Siguiente oleada (ENTER)", new Vector2(0, -200), uim.BotonContinuar);
            pT.SetActive(false);

            // Pausa, Victoria, Derrota
            var pP = Ventana("Panel_Pausa", lienzo.transform, new Vector2(700, 420));
            Txt("Titulo", pP.transform, "PAUSA", 60, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(600, 80));
            Boton("Boton_Reanudar", pP.transform, "Reanudar (ESC)", new Vector2(0, -10), uim.BotonReanudar);
            Boton("Boton_Menu", pP.transform, "Menú principal", new Vector2(0, -120), uim.BotonMenu);
            pP.SetActive(false);
            var pV = Ventana("Panel_Victoria", lienzo.transform, new Vector2(800, 420));
            Txt("Titulo", pV.transform, "¡VICTORIA!", 70, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(700, 90));
            Boton("Boton_Resultado", pV.transform, "Ver resultado", new Vector2(0, -80), uim.BotonResultado);
            pV.SetActive(false);
            var pD = Ventana("Panel_Derrota", lienzo.transform, new Vector2(800, 460));
            Txt("Titulo", pD.transform, "DERROTA", 70, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(700, 90));
            Boton("Boton_Reintentar", pD.transform, "Reintentar", new Vector2(0, -30), uim.BotonReintentar);
            Boton("Boton_Resultado", pD.transform, "Ver resultado", new Vector2(0, -140), uim.BotonResultado);
            pD.SetActive(false);

            Set(uim, "panelHUD", pHUD); Set(uim, "panelTienda", pT); Set(uim, "panelPausa", pP); Set(uim, "panelVictoria", pV); Set(uim, "panelDerrota", pD);
            Set(uim, "ayudaTablero", ayuda.gameObject); Set(uim, "textoAviso", aviso); Set(uim, "textoMensajeTienda", msg); Set(uim, "tienda", shop);
            return uim;
        }

        static void EscenaMenu(string ruta)
        {
            var esc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CamaraSimple();
            AgregarLuzGlobal();
            var f = Vacio("Fondo", null); f.transform.position = new Vector3(0f, 0f, 0f);
            var sr = f.AddComponent<SpriteRenderer>(); sr.sprite = Spr(ART + "Fondos/Arena1/bg_arena1_capa_lejana"); sr.sortingLayerName = "Fondo";
            Vacio("GameSession", null).AddComponent<GameSession>();
            var grupo = Vacio("--- Interfaz", null);
            var lienzo = Lienzo(grupo.transform);
            Sistema(grupo.transform);
            var mc = lienzo.AddComponent<MenuController>();
            var t = Txt("Titulo", lienzo.transform, "ZOMBIE SLUG", 120, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1400, 160));
            t.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, -5);
            Boton("Boton_Jugar", lienzo.transform, "Jugar", new Vector2(-300, -40), mc.Jugar);
            Boton("Boton_Salir", lienzo.transform, "Salir", new Vector2(-300, -160), mc.Salir);
            var r = Txt("Texto_Ranking", lienzo.transform, "MEJORES PUNTAJES", 32, TextAnchor.UpperLeft, new Vector2(0.5f, 0.5f), new Vector2(330, -60), new Vector2(560, 340));
            r.gameObject.AddComponent<Outline>();
            Set(mc, "textoRanking", r);
            EditorSceneManager.SaveScene(esc, ruta);
        }

        static void EscenaResultado(string ruta)
        {
            var esc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CamaraSimple();
            var grupo = Vacio("--- Interfaz", null);
            var lienzo = Lienzo(grupo.transform);
            Sistema(grupo.transform);
            var rc = lienzo.AddComponent<ResultadoController>();
            var t = Txt("Texto_Resultado", lienzo.transform, "RESULTADO", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1200, 330));
            var campo = Campo(lienzo.transform, new Vector2(0, -40));
            var g = Boton("Boton_Guardar", lienzo.transform, "Guardar iniciales", new Vector2(0, -150), rc.Guardar);
            Boton("Boton_Menu", lienzo.transform, "Menú principal", new Vector2(0, -270), rc.VolverAlMenu);
            Set(rc, "textoResultado", t); Set(rc, "campoIniciales", campo); Set(rc, "botonGuardar", g);
            EditorSceneManager.SaveScene(esc, ruta);
        }

        // ---------------------------------------------------------------- utilidades de UI
        static GameObject Lienzo(Transform padre)
        {
            var go = new GameObject("Lienzo", typeof(RectTransform)); go.transform.SetParent(padre); go.layer = 5;
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; cs.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static void Sistema(Transform padre)
        {
            var es = new GameObject("EventSystem"); es.transform.SetParent(padre);
            es.AddComponent<EventSystem>();
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) es.AddComponent(t); else es.AddComponent<StandaloneInputModule>();
        }

        static RectTransform Rect(GameObject go, Transform padre, Vector2 ancla, Vector2 pos, Vector2 tam)
        {
            go.layer = 5;
            var rt = go.GetComponent<RectTransform>(); if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.SetParent(padre, false);
            rt.anchorMin = ancla; rt.anchorMax = ancla; rt.pivot = ancla; rt.anchoredPosition = pos; rt.sizeDelta = tam;
            return rt;
        }

        static GameObject Panel(string n, Transform padre, Color col)
        {
            var go = new GameObject(n, typeof(RectTransform)); go.layer = 5;
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(padre, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            if (col.a > 0f) go.AddComponent<Image>().color = col;
            return go;
        }

        static GameObject Ventana(string n, Transform padre, Vector2 tam)
        {
            var go = new GameObject(n, typeof(RectTransform));
            Rect(go, padre, new Vector2(0.5f, 0.5f), Vector2.zero, tam).pivot = new Vector2(0.5f, 0.5f);
            go.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.11f, 0.94f);
            go.AddComponent<Outline>().effectColor = new Color(0.93f, 0.84f, 0.67f);
            return go;
        }

        static Image Img(string n, Transform padre, Color col, Vector2 ancla, Vector2 pos, Vector2 tam)
        {
            var go = new GameObject(n, typeof(RectTransform));
            Rect(go, padre, ancla, pos, tam);
            var i = go.AddComponent<Image>(); i.color = col;
            return i;
        }

        static Text Txt(string n, Transform padre, string texto, int tam, TextAnchor al, Vector2 ancla, Vector2 pos, Vector2 caja)
        {
            var go = new GameObject(n, typeof(RectTransform));
            Rect(go, padre, ancla, pos, caja);
            var t = go.AddComponent<Text>();
            t.font = Fuente(); t.text = texto; t.fontSize = tam; t.alignment = al; t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        static Button Boton(string n, Transform padre, string texto, Vector2 pos, UnityAction accion)
        {
            var go = new GameObject(n, typeof(RectTransform));
            Rect(go, padre, new Vector2(0.5f, 0.5f), pos, new Vector2(420, 84)).pivot = new Vector2(0.5f, 0.5f);
            var img = go.AddComponent<Image>(); img.color = new Color(0.75f, 0.22f, 0.17f);
            var b = go.AddComponent<Button>(); b.targetGraphic = img;
            var cb = b.colors; cb.highlightedColor = new Color(1f, 0.8f, 0.6f); cb.pressedColor = new Color(0.5f, 0.1f, 0.1f); b.colors = cb;
            var t = Txt("Texto", go.transform, texto, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 80));
            t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            UnityEventTools.AddPersistentListener(b.onClick, accion);
            return b;
        }

        static InputField Campo(Transform padre, Vector2 pos)
        {
            var go = new GameObject("Campo_Iniciales", typeof(RectTransform));
            Rect(go, padre, new Vector2(0.5f, 0.5f), pos, new Vector2(300, 80)).pivot = new Vector2(0.5f, 0.5f);
            var img = go.AddComponent<Image>(); img.color = Color.white;
            var f = go.AddComponent<InputField>();
            var ph = Txt("Placeholder", go.transform, "AAA", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 70));
            ph.rectTransform.pivot = new Vector2(0.5f, 0.5f); ph.color = new Color(0.5f, 0.5f, 0.5f); ph.fontStyle = FontStyle.Italic;
            var tx = Txt("Texto", go.transform, "", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 70));
            tx.rectTransform.pivot = new Vector2(0.5f, 0.5f); tx.color = Color.black; tx.supportRichText = false;
            f.textComponent = tx; f.placeholder = ph; f.characterLimit = 3; f.targetGraphic = img;
            return f;
        }

        static void CamaraSimple()
        {
            var camGo = Vacio("Main Camera", null); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = CAM_SIZE; cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            camGo.AddComponent<AudioListener>();
        }

        // Componentes de URP agregados por nombre para no atar el script a un paquete concreto
        static void AgregarPixelPerfect(GameObject camGo)
        {
            var t = System.Type.GetType("UnityEngine.Rendering.Universal.PixelPerfectCamera, Unity.RenderPipelines.Universal.2D.Runtime")
                 ?? System.Type.GetType("UnityEngine.Rendering.Universal.PixelPerfectCamera, Unity.RenderPipelines.Universal.Runtime");
            if (t == null) { Debug.LogWarning("[Zombie Slug] PixelPerfectCamera no encontrado; se mantiene la cámara ortográfica."); return; }
            var c = camGo.AddComponent(t);
            var so = new SerializedObject(c);
            SetSo(so, "m_AssetsPPU", 32); SetSo(so, "m_RefResolutionX", 640); SetSo(so, "m_RefResolutionY", 360);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AgregarLuzGlobal()
        {
            var t = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime")
                 ?? System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (t == null) return;
            var go = Vacio("Luz_Global_2D", null);
            var c = go.AddComponent(t);
            var so = new SerializedObject(c);
            SetSo(so, "m_LightType", 4);   // Global
            var p = so.FindProperty("m_Intensity"); if (p != null) p.floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetSo(SerializedObject so, string prop, int v) { var p = so.FindProperty(prop); if (p != null) p.intValue = v; }

        // ---------------------------------------------------------------- utilidades generales
        static GameObject Vacio(string n, Transform padre) { var g = new GameObject(n); if (padre != null) g.transform.SetParent(padre); return g; }
        static int L(string capa) { return LayerMask.NameToLayer(capa); }
        static int Mascara(params string[] capas) { return LayerMask.GetMask(capas); }

        static void Set(Object comp, string prop, Object valor)
        {
            var so = new SerializedObject(comp); var p = so.FindProperty(prop);
            if (p == null) { Debug.LogError("[Zombie Slug] Campo no encontrado: " + comp.GetType().Name + "." + prop); return; }
            p.objectReferenceValue = valor; so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetInt(Object comp, string prop, int v)
        {
            var so = new SerializedObject(comp); var p = so.FindProperty(prop);
            if (p == null) { Debug.LogError("[Zombie Slug] Campo no encontrado: " + prop); return; }
            p.intValue = v; so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetF(Object comp, string prop, float v)
        {
            var so = new SerializedObject(comp); var p = so.FindProperty(prop);
            if (p == null) { Debug.LogError("[Zombie Slug] Campo no encontrado: " + prop); return; }
            p.floatValue = v; so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetArr(Object comp, string prop, Object[] vals)
        {
            var so = new SerializedObject(comp); var p = so.FindProperty(prop);
            p.arraySize = vals.Length;
            for (int i = 0; i < vals.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = vals[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
