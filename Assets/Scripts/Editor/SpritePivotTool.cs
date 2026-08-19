using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class SpritePivotTool : EditorWindow
    {
        private Texture2D m_Texture;
        private List<SpriteMetaData> m_SpriteList = new();
        private int m_CurrentIndex;
        private bool m_Dirty;

        private IMGUIContainer m_SpriteView;
        private Label m_InfoLabel;
        private Label m_CountLabel;
        private Button m_ApplyButton;

        private Vector2 m_ScrollPos;
        private float m_Zoom = 1f;

        [MenuItem("Tools/Sprite Pivot Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpritePivotTool>();
            window.titleContent = new GUIContent("Sprite Pivot Tool");
            window.minSize = new Vector2(600, 500);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;

            // 상단 바
            var topBar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 8, paddingRight = 8, paddingTop = 4, paddingBottom = 4 } };

            var texField = new UnityEditor.UIElements.ObjectField("Sprite Sheet") { objectType = typeof(Texture2D), allowSceneObjects = false };
            texField.style.flexGrow = 1;
            texField.RegisterValueChangedCallback(evt =>
            {
                m_Texture = evt.newValue as Texture2D;
                LoadSprites();
            });
            topBar.Add(texField);
            root.Add(topBar);

            // 네비게이션
            var navBar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 8, paddingRight = 8, paddingBottom = 4, justifyContent = Justify.Center } };

            var prevBtn = new Button(() => Navigate(-1)) { text = "◀ Prev" };
            prevBtn.style.width = 80;
            prevBtn.style.height = 28;
            navBar.Add(prevBtn);

            m_CountLabel = new Label("0 / 0") { style = { unityTextAlign = TextAnchor.MiddleCenter, width = 100 } };
            navBar.Add(m_CountLabel);

            var nextBtn = new Button(() => Navigate(1)) { text = "Next ▶" };
            nextBtn.style.width = 80;
            nextBtn.style.height = 28;
            navBar.Add(nextBtn);

            var zoomIn = new Button(() => SetZoom(m_Zoom + 0.5f)) { text = "+" };
            zoomIn.style.width = 30;
            zoomIn.style.height = 28;
            zoomIn.style.marginLeft = 20;
            navBar.Add(zoomIn);

            var zoomLabel = new Label("Zoom");
            zoomLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            zoomLabel.style.width = 40;
            navBar.Add(zoomLabel);

            var zoomOut = new Button(() => SetZoom(m_Zoom - 0.5f)) { text = "-" };
            zoomOut.style.width = 30;
            zoomOut.style.height = 28;
            navBar.Add(zoomOut);

            root.Add(navBar);

            // 자동 피벗 버튼
            var autoBar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 8, paddingRight = 8, paddingBottom = 4, justifyContent = Justify.Center } };

            var autoCurrent = new Button(AutoPivotCurrent) { text = "Auto (Current)", tooltip = "현재 스프라이트 알파 기준 BottomCenter" };
            autoCurrent.style.height = 24;
            autoBar.Add(autoCurrent);

            var autoAll = new Button(AutoPivotAll) { text = "Auto (All)", tooltip = "전체 스프라이트 알파 기준 BottomCenter" };
            autoAll.style.height = 24;
            autoAll.style.marginLeft = 4;
            autoBar.Add(autoAll);

            root.Add(autoBar);

            // 스프라이트 뷰 (IMGUI — 이미지 + 클릭 처리)
            m_SpriteView = new IMGUIContainer(OnSpriteViewGUI);
            m_SpriteView.style.flexGrow = 1;
            m_SpriteView.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            root.Add(m_SpriteView);

            // 하단 정보 + 적용
            var bottomBar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 8, paddingRight = 8, paddingTop = 4, paddingBottom = 4 } };

            m_InfoLabel = new Label("Texture를 선택하세요.") { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
            bottomBar.Add(m_InfoLabel);

            m_ApplyButton = new Button(OnApply) { text = "Apply All" };
            m_ApplyButton.style.width = 100;
            m_ApplyButton.style.height = 28;
            m_ApplyButton.SetEnabled(false);
            bottomBar.Add(m_ApplyButton);

            root.Add(bottomBar);
        }

        private void LoadSprites()
        {
            m_SpriteList.Clear();
            m_CurrentIndex = 0;
            m_Dirty = false;

            if (m_Texture == null)
            {
                m_InfoLabel.text = "Texture를 선택하세요.";
                m_CountLabel.text = "0 / 0";
                m_ApplyButton.SetEnabled(false);
                return;
            }

            string path = AssetDatabase.GetAssetPath(m_Texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                m_InfoLabel.text = "Multiple 모드 스프라이트만 지원합니다.";
                return;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                m_InfoLabel.text = "Read/Write 활성화됨. 다시 선택하세요.";
                return;
            }

            foreach (var meta in importer.spritesheet)
                m_SpriteList.Add(meta);

            UpdateInfo();
        }

        private void Navigate(int dir)
        {
            if (m_SpriteList.Count == 0) return;
            m_CurrentIndex = Mathf.Clamp(m_CurrentIndex + dir, 0, m_SpriteList.Count - 1);
            UpdateInfo();
            m_SpriteView.MarkDirtyRepaint();
        }

        private void SetZoom(float z)
        {
            m_Zoom = Mathf.Clamp(z, 0.5f, 8f);
            m_SpriteView.MarkDirtyRepaint();
        }

        private void UpdateInfo()
        {
            if (m_SpriteList.Count == 0)
            {
                m_InfoLabel.text = "";
                m_CountLabel.text = "0 / 0";
                return;
            }

            var meta = m_SpriteList[m_CurrentIndex];
            m_CountLabel.text = $"{m_CurrentIndex + 1} / {m_SpriteList.Count}";
            m_InfoLabel.text = $"{meta.name}  |  Pivot: ({meta.pivot.x:F3}, {meta.pivot.y:F3})  |  {(int)meta.rect.width}x{(int)meta.rect.height}";
        }

        private void OnSpriteViewGUI()
        {
            if (m_Texture == null || m_SpriteList.Count == 0) return;

            var meta = m_SpriteList[m_CurrentIndex];
            Rect spriteRect = meta.rect;

            float drawW = spriteRect.width * m_Zoom;
            float drawH = spriteRect.height * m_Zoom;

            Rect viewRect = m_SpriteView.contentRect;
            float totalW = Mathf.Max(drawW + 40, viewRect.width);
            float totalH = Mathf.Max(drawH + 40, viewRect.height);

            m_ScrollPos = GUI.BeginScrollView(new Rect(0, 0, viewRect.width, viewRect.height), m_ScrollPos, new Rect(0, 0, totalW, totalH));

            float offsetX = (totalW - drawW) * 0.5f;
            float offsetY = (totalH - drawH) * 0.5f;
            Rect drawRect = new Rect(offsetX, offsetY, drawW, drawH);

            // 체크 배경
            DrawCheckerboard(drawRect);

            // 스프라이트 그리기 (UV 좌표)
            Rect uvRect = new Rect(
                spriteRect.x / m_Texture.width,
                spriteRect.y / m_Texture.height,
                spriteRect.width / m_Texture.width,
                spriteRect.height / m_Texture.height
            );
            GUI.DrawTextureWithTexCoords(drawRect, m_Texture, uvRect);

            // 피벗 표시
            Vector2 pivot = meta.pivot;
            float pivotScreenX = drawRect.x + pivot.x * drawW;
            float pivotScreenY = drawRect.y + (1f - pivot.y) * drawH;

            // 십자선
            Handles.color = Color.red;
            float crossSize = 12f;
            Handles.DrawLine(new Vector3(pivotScreenX - crossSize, pivotScreenY, 0), new Vector3(pivotScreenX + crossSize, pivotScreenY, 0));
            Handles.DrawLine(new Vector3(pivotScreenX, pivotScreenY - crossSize, 0), new Vector3(pivotScreenX, pivotScreenY + crossSize, 0));

            // 원
            Handles.DrawWireDisc(new Vector3(pivotScreenX, pivotScreenY, 0), Vector3.forward, 6f);

            // 클릭으로 피벗 설정
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && drawRect.Contains(e.mousePosition))
            {
                float newPivotX = (e.mousePosition.x - drawRect.x) / drawW;
                float newPivotY = 1f - (e.mousePosition.y - drawRect.y) / drawH;

                newPivotX = Mathf.Clamp01(newPivotX);
                newPivotY = Mathf.Clamp01(newPivotY);

                var updated = m_SpriteList[m_CurrentIndex];
                updated.pivot = new Vector2(newPivotX, newPivotY);
                updated.alignment = (int)SpriteAlignment.Custom;
                m_SpriteList[m_CurrentIndex] = updated;

                m_Dirty = true;
                m_ApplyButton.SetEnabled(true);
                UpdateInfo();
                e.Use();
            }

            // 스크롤 줌
            if (e.type == EventType.ScrollWheel && viewRect.Contains(e.mousePosition))
            {
                SetZoom(m_Zoom - e.delta.y * 0.1f);
                e.Use();
            }

            GUI.EndScrollView();
        }

        private void DrawCheckerboard(Rect rect)
        {
            float cellSize = 8f;
            Color c1 = new Color(0.25f, 0.25f, 0.25f);
            Color c2 = new Color(0.35f, 0.35f, 0.35f);

            for (float y = rect.y; y < rect.yMax; y += cellSize)
            {
                for (float x = rect.x; x < rect.xMax; x += cellSize)
                {
                    int ix = Mathf.FloorToInt((x - rect.x) / cellSize);
                    int iy = Mathf.FloorToInt((y - rect.y) / cellSize);
                    EditorGUI.DrawRect(new Rect(x, y, cellSize, cellSize), (ix + iy) % 2 == 0 ? c1 : c2);
                }
            }
        }

        private void AutoPivotCurrent()
        {
            if (m_Texture == null || m_SpriteList.Count == 0) return;

            var meta = m_SpriteList[m_CurrentIndex];
            Vector2 pivot = CalcAlphaPivot(meta.rect);

            meta.pivot = pivot;
            meta.alignment = (int)SpriteAlignment.Custom;
            m_SpriteList[m_CurrentIndex] = meta;

            m_Dirty = true;
            m_ApplyButton.SetEnabled(true);
            UpdateInfo();
            m_SpriteView.MarkDirtyRepaint();
        }

        private void AutoPivotAll()
        {
            if (m_Texture == null || m_SpriteList.Count == 0) return;

            for (int i = 0; i < m_SpriteList.Count; i++)
            {
                var meta = m_SpriteList[i];
                meta.pivot = CalcAlphaPivot(meta.rect);
                meta.alignment = (int)SpriteAlignment.Custom;
                m_SpriteList[i] = meta;
            }

            m_Dirty = true;
            m_ApplyButton.SetEnabled(true);
            UpdateInfo();
            m_SpriteView.MarkDirtyRepaint();
        }

        private Vector2 CalcAlphaPivot(Rect spriteRect)
        {
            int sx = Mathf.FloorToInt(spriteRect.x);
            int sy = Mathf.FloorToInt(spriteRect.y);
            int w = Mathf.FloorToInt(spriteRect.width);
            int h = Mathf.FloorToInt(spriteRect.height);

            Color[] pixels = m_Texture.GetPixels(sx, sy, w, h);

            int minX = w, maxX = 0, minY = h;
            bool hasPixel = false;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a > 0.01f)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        hasPixel = true;
                    }
                }
            }

            if (!hasPixel)
                return new Vector2(0.5f, 0.5f);

            float centerX = (minX + maxX) * 0.5f / w;
            float bottomY = (float)minY / h;

            return new Vector2(centerX, bottomY);
        }

        private void OnApply()
        {
            if (m_Texture == null || m_SpriteList.Count == 0) return;

            Vector2 currentPivot = m_SpriteList[m_CurrentIndex].pivot;

            for (int i = 0; i < m_SpriteList.Count; i++)
            {
                var meta = m_SpriteList[i];
                meta.alignment = (int)SpriteAlignment.Custom;
                meta.pivot = currentPivot;
                m_SpriteList[i] = meta;
            }

            string path = AssetDatabase.GetAssetPath(m_Texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.spritesheet = m_SpriteList.ToArray();
            EditorUtility.SetDirty(importer);
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            m_Dirty = false;
            m_ApplyButton.SetEnabled(false);
            m_InfoLabel.text = $"피벗 ({currentPivot.x:F3}, {currentPivot.y:F3}) → {m_SpriteList.Count}개 전체 적용 완료!";
        }
    }
}
