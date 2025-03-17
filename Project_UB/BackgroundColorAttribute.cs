using System;
using UnityEngine;
using UnityEditor;

namespace UB
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HeaderAttribute : PropertyAttribute
    {
        public readonly string header;
        public readonly string colorString;
        public readonly Color color;
        public readonly float textHeightIncrease;
        public readonly HeaderAlign headerAlign = HeaderAlign.Left;

        private const float DefaultSize = 2f;
        public const float HeaderMaxSize = 8f;
        public enum HeaderAlign
        {
            Left,
            Center,
            Right,
        }

        public HeaderAttribute(string header)
        {
            this.header = header;
            this.textHeightIncrease = DefaultSize;
        }

        public HeaderAttribute(string header, int order)
        {
            this.header = header;
            this.textHeightIncrease = DefaultSize;
            this.order = order;
        }
        public HeaderAttribute(string header, string colorString) : this(header, DefaultSize, colorString)
        {

        }

        public HeaderAttribute(string header, HeaderAlign headerAlign) : this(header, DefaultSize, "white")
        {
            this.headerAlign = headerAlign;
        }

        public HeaderAttribute(string header, string colorString, HeaderAlign headerAlign) : this(header, DefaultSize, colorString)
        {
            this.headerAlign = headerAlign;
        }

        public HeaderAttribute(string header, float textHeightIncrease, string colorString, HeaderAlign headerAlign) : this(header, textHeightIncrease, colorString)
        {
            this.headerAlign = headerAlign;
        }

        public HeaderAttribute(string header, float textHeightIncrease, string colorString, HeaderAlign headerAlign, int order) : this(header, textHeightIncrease, colorString)
        {
            this.headerAlign = headerAlign;
            this.order = order;
        }


        public HeaderAttribute(string header, float textHeightIncrease = DefaultSize, string colorString = "lightblue")
        {
            this.header = header;
            this.colorString = colorString;

            //Size Range
            float size = Mathf.Max(DefaultSize, textHeightIncrease);
            this.textHeightIncrease = Mathf.Min(size, HeaderMaxSize);

            if (string.IsNullOrEmpty(header))
                this.textHeightIncrease = DefaultSize;

            if (ColorUtility.TryParseHtmlString(colorString, out this.color)) return;

            this.color = new Color(173f, 216f, 230f);
            this.colorString = "lightblue";
        }
    }

    [CustomPropertyDrawer(typeof(HeaderAttribute))]
    public class HeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {

            if (!(attribute is HeaderAttribute headerAttribute)) return;

            if (string.IsNullOrEmpty(headerAttribute.header))
            {
                position.height = headerAttribute.textHeightIncrease;
                EditorGUI.DrawRect(position, headerAttribute.color);
                return;
            }

            position = EditorGUI.IndentedRect(position);    //IndentRect  First Check

            GUIStyle style = new GUIStyle(EditorStyles.label) { richText = true };
            GUIContent label = new GUIContent(
                $"<color={headerAttribute.colorString}><size={style.fontSize + headerAttribute.textHeightIncrease}><b>[{headerAttribute.header}]</b></size></color>");

            Vector2 textSize = style.CalcSize(label);

            float separatorWidth = (position.width - textSize.x) / 2f;
            float separatorStartY = position.yMin + (position.height / 2f) - (headerAttribute.textHeightIncrease / 2f);

            Rect prefixRect = new Rect();
            Rect postRect = new Rect();
            Rect labelRect;
            float IndentedX = (position.x / 4f);
            float labelWidth = (textSize.x + position.x);
            switch (headerAttribute.headerAlign)
            {
                case HeaderAttribute.HeaderAlign.Center:
                    {

                        prefixRect = new Rect(position.xMin, separatorStartY, separatorWidth, headerAttribute.textHeightIncrease);
                        labelRect = new Rect(position.xMin + separatorWidth - IndentedX, position.yMin, labelWidth, position.height);
                        postRect = new Rect(position.xMin + separatorWidth + textSize.x, separatorStartY, separatorWidth, headerAttribute.textHeightIncrease);
                    }
                    break;
                case HeaderAttribute.HeaderAlign.Left:
                default:
                    {
                        labelRect = new Rect(position.xMin - IndentedX, position.yMin, labelWidth, position.height);
                        postRect = new Rect(position.xMin + textSize.x, separatorStartY, separatorWidth * 2f, headerAttribute.textHeightIncrease);
                    }
                    break;
                case HeaderAttribute.HeaderAlign.Right:
                    {

                        prefixRect = new Rect(position.xMin, separatorStartY, separatorWidth * 2f, headerAttribute.textHeightIncrease);
                        labelRect = new Rect(prefixRect.width + position.xMin - IndentedX, position.yMin, labelWidth, position.height);
                    }
                    break;
            }

            EditorGUI.DrawRect(prefixRect, headerAttribute.color);
            EditorGUI.LabelField(labelRect, label, style);
            EditorGUI.DrawRect(postRect, headerAttribute.color);
        }

        /// <summary>
        /// 높이 기본값 1.5배로 처리
        /// </summary>
        /// <returns></returns>
        public override float GetHeight()
        {
            if ((attribute is HeaderAttribute headerAttribute) == false)
                return EditorGUIUtility.singleLineHeight * 1.5f;

            return GetTotalHeight(attribute as HeaderAttribute);
        }

        /// <summary>
        /// 줄바꿈 계산
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private float GetTotalHeight(HeaderAttribute attr)
        {
            int line = (attr.header.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length);
            return (EditorGUIUtility.singleLineHeight * Mathf.Max(line, 1)) + (EditorGUIUtility.singleLineHeight * 0.5f);
        }
    }
}