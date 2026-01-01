using UnityEngine;
using UnityEngine.UIElements;

namespace MilosAdventure.UI.CustomElements
{
    /// <summary>
    /// Custom VisualElement for rendering planet/star icons in UI Toolkit.
    /// Supports selection state, clickable events, and adaptive sizing.
    /// </summary>
    public class PlanetIconElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PlanetIconElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription _planetId = new UxmlStringAttributeDescription
            {
                name = "planet-id",
                defaultValue = ""
            };

            private UxmlColorAttributeDescription _iconColor = new UxmlColorAttributeDescription
            {
                name = "icon-color",
                defaultValue = Color.white
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = ve as PlanetIconElement;

                element.PlanetId = _planetId.GetValueFromBag(bag, cc);
                element.IconColor = _iconColor.GetValueFromBag(bag, cc);
            }
        }

        private string _planetId;
        private Color _iconColor;

        public string PlanetId
        {
            get => _planetId;
            set
            {
                _planetId = value;
                this.name = $"planet-icon-{value}";
            }
        }

        public Color IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                style.unityBackgroundImageTintColor = value;
            }
        }

        public PlanetIconElement()
        {
            AddToClassList("planet-icon");

            style.position = Position.Absolute;
            style.width = 16;
            style.height = 16;
        }

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                AddToClassList("planet-icon--selected");
            }
            else
            {
                RemoveFromClassList("planet-icon--selected");
            }
        }
    }
}
