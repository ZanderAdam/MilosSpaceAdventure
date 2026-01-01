using NUnit.Framework;
using UnityEngine;
using MilosAdventure.UI.Utils;

namespace MilosAdventure.Tests.EditMode.UI
{
    public class CoordinateConverterTests
    {
        private const float TOLERANCE = 0.01f;

        [Test]
        public void WorldToUI_CenterPosition_ReturnsCenterOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldCenter = Vector3.zero;

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldCenter, worldSize, uiSize);

            Assert.AreEqual(75f, result.x, TOLERANCE, "Center X should be at UI center");
            Assert.AreEqual(75f, result.y, TOLERANCE, "Center Y should be at UI center");
        }

        [Test]
        public void WorldToUI_TopLeftCorner_ReturnsTopLeftOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldTopLeft = new Vector3(-50, 50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldTopLeft, worldSize, uiSize);

            Assert.AreEqual(0f, result.x, TOLERANCE, "Top-left X should be 0");
            Assert.AreEqual(0f, result.y, TOLERANCE, "Top-left Y should be 0");
        }

        [Test]
        public void WorldToUI_TopRightCorner_ReturnsTopRightOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldTopRight = new Vector3(50, 50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldTopRight, worldSize, uiSize);

            Assert.AreEqual(150f, result.x, TOLERANCE, "Top-right X should be UI width");
            Assert.AreEqual(0f, result.y, TOLERANCE, "Top-right Y should be 0");
        }

        [Test]
        public void WorldToUI_BottomLeftCorner_ReturnsBottomLeftOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldBottomLeft = new Vector3(-50, -50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldBottomLeft, worldSize, uiSize);

            Assert.AreEqual(0f, result.x, TOLERANCE, "Bottom-left X should be 0");
            Assert.AreEqual(150f, result.y, TOLERANCE, "Bottom-left Y should be UI height");
        }

        [Test]
        public void WorldToUI_BottomRightCorner_ReturnsBottomRightOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldBottomRight = new Vector3(50, -50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldBottomRight, worldSize, uiSize);

            Assert.AreEqual(150f, result.x, TOLERANCE, "Bottom-right X should be UI width");
            Assert.AreEqual(150f, result.y, TOLERANCE, "Bottom-right Y should be UI height");
        }

        [Test]
        public void WorldToUI_TopEdge_ReturnsTopEdgeOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldTopEdge = new Vector3(0, 50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldTopEdge, worldSize, uiSize);

            Assert.AreEqual(75f, result.x, TOLERANCE, "Top-edge X should be at UI center");
            Assert.AreEqual(0f, result.y, TOLERANCE, "Top-edge Y should be 0");
        }

        [Test]
        public void WorldToUI_BottomEdge_ReturnsBottomEdgeOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldBottomEdge = new Vector3(0, -50, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldBottomEdge, worldSize, uiSize);

            Assert.AreEqual(75f, result.x, TOLERANCE, "Bottom-edge X should be at UI center");
            Assert.AreEqual(150f, result.y, TOLERANCE, "Bottom-edge Y should be UI height");
        }

        [Test]
        public void WorldToUI_LeftEdge_ReturnsLeftEdgeOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldLeftEdge = new Vector3(-50, 0, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldLeftEdge, worldSize, uiSize);

            Assert.AreEqual(0f, result.x, TOLERANCE, "Left-edge X should be 0");
            Assert.AreEqual(75f, result.y, TOLERANCE, "Left-edge Y should be at UI center");
        }

        [Test]
        public void WorldToUI_RightEdge_ReturnsRightEdgeOfUI()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 worldRightEdge = new Vector3(50, 0, 0);

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldRightEdge, worldSize, uiSize);

            Assert.AreEqual(150f, result.x, TOLERANCE, "Right-edge X should be UI width");
            Assert.AreEqual(75f, result.y, TOLERANCE, "Right-edge Y should be at UI center");
        }

        [Test]
        public void WorldToUI_LargeUISize_ScalesCorrectly()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(800, 800);
            Vector3 worldCenter = Vector3.zero;

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldCenter, worldSize, uiSize);

            Assert.AreEqual(400f, result.x, TOLERANCE, "Center X should scale to large UI");
            Assert.AreEqual(400f, result.y, TOLERANCE, "Center Y should scale to large UI");
        }

        [Test]
        public void WorldToUI_SmallUISize_ScalesCorrectly()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(120, 120);
            Vector3 worldCenter = Vector3.zero;

            Vector2 result = CoordinateConverter.WorldToUIPosition(worldCenter, worldSize, uiSize);

            Assert.AreEqual(60f, result.x, TOLERANCE, "Center X should scale to small UI");
            Assert.AreEqual(60f, result.y, TOLERANCE, "Center Y should scale to small UI");
        }

        [Test]
        public void UIToWorld_CenterOfUI_ReturnsWorldCenter()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector2 uiCenter = new Vector2(75, 75);

            Vector2 result = CoordinateConverter.UIToWorldPosition(uiCenter, worldSize, uiSize);

            Assert.AreEqual(0f, result.x, TOLERANCE, "UI center should map to world center X");
            Assert.AreEqual(0f, result.y, TOLERANCE, "UI center should map to world center Y");
        }

        [Test]
        public void UIToWorld_TopLeftOfUI_ReturnsTopLeftWorld()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector2 uiTopLeft = new Vector2(0, 0);

            Vector2 result = CoordinateConverter.UIToWorldPosition(uiTopLeft, worldSize, uiSize);

            Assert.AreEqual(-50f, result.x, TOLERANCE, "UI top-left should map to world top-left X");
            Assert.AreEqual(50f, result.y, TOLERANCE, "UI top-left should map to world top-left Y");
        }

        [Test]
        public void UIToWorld_BottomRightOfUI_ReturnsBottomRightWorld()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector2 uiBottomRight = new Vector2(150, 150);

            Vector2 result = CoordinateConverter.UIToWorldPosition(uiBottomRight, worldSize, uiSize);

            Assert.AreEqual(50f, result.x, TOLERANCE, "UI bottom-right should map to world bottom-right X");
            Assert.AreEqual(-50f, result.y, TOLERANCE, "UI bottom-right should map to world bottom-right Y");
        }

        [Test]
        public void WorldToUIScale_SmallObject_ScalesCorrectly()
        {
            float worldObjectSize = 5f;
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);

            float result = CoordinateConverter.WorldToUIScale(worldObjectSize, worldSize, uiSize);

            Assert.AreEqual(7.5f, result, TOLERANCE, "Small object should scale proportionally");
        }

        [Test]
        public void WorldToUIScale_LargeObject_ScalesCorrectly()
        {
            float worldObjectSize = 20f;
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(800, 800);

            float result = CoordinateConverter.WorldToUIScale(worldObjectSize, worldSize, uiSize);

            Assert.AreEqual(160f, result, TOLERANCE, "Large object should scale proportionally");
        }

        [Test]
        public void WorldToUI_RoundTrip_ReturnsOriginalPosition()
        {
            Vector2 worldSize = new Vector2(100, 100);
            Vector2 uiSize = new Vector2(150, 150);
            Vector3 originalWorld = new Vector3(25, -15, 0);

            Vector2 uiPos = CoordinateConverter.WorldToUIPosition(originalWorld, worldSize, uiSize);
            Vector2 worldPos = CoordinateConverter.UIToWorldPosition(uiPos, worldSize, uiSize);

            Assert.AreEqual(originalWorld.x, worldPos.x, TOLERANCE, "Round-trip X should match");
            Assert.AreEqual(originalWorld.y, worldPos.y, TOLERANCE, "Round-trip Y should match");
        }
    }
}
