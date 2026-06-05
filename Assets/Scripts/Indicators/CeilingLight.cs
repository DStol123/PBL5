using UnityEngine;
using UnityEngine.UI;

public class CeilingLight : MonoBehaviour
{
    [SerializeField] private float lightSize;

    [SerializeField] private GameObject lightAnchor;

    [SerializeField] private float rectWidth;
    [SerializeField] private float rectHeight;
    [SerializeField] private float rectRadius;

    [SerializeField][Range(0f, 1f)] private float alpha;

    [SerializeField] private int lightCount;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite pointSprite;

    [SerializeField] private GameObject userAvatar;

    private GameObject[] points;
    private Image[] images;

    private bool activated;

    private GameObject[] invisibleObjects;

    private void Start()
    {
        InitializePoints();
    }

    private void FixedUpdate()
    {
        ChangePointColors();
    }

    private void InitializePoints()
    {
        if (lightCount == 0) return;

        activated = true;

        Vector2[] positions = PointsOnRoundedRectangle();

        Vector3 center = lightAnchor.transform.localPosition;

        points = new GameObject[lightCount];
        images = new Image[lightCount];
        invisibleObjects = new GameObject[lightCount];


        for (int i = 0; i < lightCount; i++)
        {
            GameObject imageObject = new GameObject("Light Image");

            invisibleObjects[i] = new GameObject("Light Pos Marker");

            invisibleObjects[i].transform.SetParent(userAvatar.transform, false);

            points[i] = imageObject;

            imageObject.transform.SetParent(canvas.transform, false);

            Image attachedImage = imageObject.AddComponent<Image>();

            images[i] = attachedImage;

            attachedImage.sprite = pointSprite;

            imageObject.transform.localPosition = center + new Vector3(positions[i].x, positions[i].y, 0f);
            invisibleObjects[i].transform.localPosition = center + new Vector3(positions[i].x, positions[i].y, 0f);
        }
    }

    private Vector2[] PointsOnRoundedRectangle()
    {
        float straightWidth = rectWidth - 2f * rectRadius;
        float straightHeight = rectHeight - 2f * rectRadius;
        float arcLength = Mathf.PI * rectRadius / 2f;

        float perimeter = 2f * (straightWidth + straightHeight) + 4f * arcLength;

        float step = perimeter / lightCount;

        Vector2[] returnPoints = new Vector2[lightCount];

        Vector2 start = new Vector2(rectRadius, straightHeight + 2f * rectRadius);

        float covered = 0f;

        bool cornerPlaced = false;
        bool cornerCorrected = false;

        int currentSegmentIndex = 0;

        float remainder = 0f;

        for (int i = 0; i < lightCount; i++)
        {
            covered = i * step;
            if(covered <= straightWidth)
            {
                //Top segment
                returnPoints[i] = start + new Vector2(step, 0f) * i;

                continue;
            }

            if(covered > straightWidth && covered <= straightWidth + arcLength)
            {
                //Top right corner
                if (covered < straightWidth + step)
                {
                    currentSegmentIndex = 0;
                    remainder = covered - straightWidth;
                }

                cornerPlaced = true;

                float numerator = remainder + step * currentSegmentIndex;

                float angle = 0.5f * Mathf.PI - numerator / rectRadius;

                Vector2 origin = new Vector2(straightWidth + rectRadius, straightHeight + rectRadius);

                returnPoints[i] = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rectRadius;

                currentSegmentIndex++;

                remainder = covered > straightWidth + arcLength ? 0f : remainder;
                continue;
            }

            if(covered > straightWidth + arcLength && covered <= straightWidth + arcLength + straightHeight)
            {
                //Right segment
                if (covered < straightWidth + arcLength + step)
                {
                    currentSegmentIndex = 0;
                    remainder = covered - (straightWidth + arcLength);
                }

                Vector2 origin = new Vector2(straightWidth + 2f * rectRadius, straightHeight + rectRadius);

                if(!cornerPlaced && !cornerCorrected)
                {
                    remainder -= rectRadius;

                    returnPoints[i] = origin - Vector2.up * remainder;

                    currentSegmentIndex++;
                    cornerCorrected = true;
                    continue;
                }
                cornerPlaced = false;
                cornerCorrected = true;

                returnPoints[i] = origin - Vector2.up * step * currentSegmentIndex - Vector2.up * remainder;

                currentSegmentIndex++;

                remainder = covered > straightWidth + arcLength + straightHeight ? 0f : remainder;
                continue;
            }

            if (covered > straightWidth + arcLength + straightHeight && covered <= straightWidth + 2f * arcLength + straightHeight)
            {
                //Bottom right corner
                if(covered < straightWidth + arcLength + straightHeight + step)
                {
                    currentSegmentIndex = 0;
                    remainder = covered - (straightWidth + arcLength + straightHeight);
                }

                cornerCorrected = false;
                cornerPlaced = true;

                Vector2 origin = new Vector2(straightWidth +  rectRadius, rectRadius);

                float numerator = remainder + step * currentSegmentIndex;

                float angle = 0 - numerator / rectRadius;

                returnPoints[i] = origin - new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * rectRadius;

                currentSegmentIndex++;
                
                remainder = covered > straightWidth + 2f * arcLength + straightHeight ? 0f : remainder;
                continue;
            }

            if(covered > straightWidth + 2f * arcLength + straightHeight && covered <= 2f* straightWidth + 2f * arcLength + straightHeight)
            {
                //Bottom segment
                Vector2 origin = new Vector2(rectRadius + straightWidth, 0f);

                if(covered < straightWidth + 2f * arcLength + straightHeight + step)
                {
                    currentSegmentIndex = 0;
                    remainder = covered - (straightWidth + 2f * arcLength + straightHeight);
                }

                if(!cornerPlaced && !cornerCorrected)
                {
                    remainder -= rectRadius;

                    returnPoints[i] = origin - Vector2.left * remainder;
                    cornerCorrected = true;

                    currentSegmentIndex++;
                    continue;
                }

                returnPoints[i] = origin - Vector2.right * remainder - Vector2.right * step * currentSegmentIndex;

                cornerPlaced = false;
                cornerCorrected = true;
                currentSegmentIndex++;

                remainder = covered > 2f * straightWidth + 2f * arcLength + straightHeight ? 0f : remainder;
                continue;
            }

            if(covered > straightWidth * 2f + arcLength * 2f + straightHeight && covered <= straightWidth * 2f + arcLength * 3f + straightHeight)
            {
                //Bottom left corner
                cornerCorrected = false;
                cornerPlaced = true;

                Vector2 origin = new Vector2(rectRadius, rectRadius);

                if(covered < straightWidth * 2f + arcLength * 2f + straightHeight + step)
                {
                    remainder = covered - (straightWidth * 2f + arcLength * 2f + straightHeight);
                    currentSegmentIndex = 0;
                }

                float numerator = remainder + step * currentSegmentIndex;

                float angle = 3f * Mathf.PI / 2f - numerator / rectRadius;

                returnPoints[i] = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rectRadius;
                remainder = covered > straightWidth * 2f + arcLength * 3f + straightHeight ? 0f : remainder;
                currentSegmentIndex++;
                continue;
            }

            if(covered > 2f * straightWidth + 3f * arcLength + straightHeight && covered <= 2f * straightWidth + 3f * arcLength + 2f * straightHeight)
            {
                //Left segment
                Vector2 origin = new Vector2(0f, rectRadius);
                cornerCorrected = false;

                if(covered < 2f * straightWidth + 3f * arcLength + straightHeight + step)
                {
                    remainder = covered - (2f * straightWidth + 3f * arcLength + straightHeight);
                    currentSegmentIndex = 0;
                }

                if(!cornerCorrected && !cornerPlaced)
                {
                    remainder -= rectRadius;

                    returnPoints[i] = origin + Vector2.up * remainder;

                    currentSegmentIndex++;
                    cornerCorrected = true;
                    continue;
                }

                returnPoints[i] = origin + Vector2.up * remainder + Vector2.up * step * currentSegmentIndex;

                remainder = covered > 2f * straightWidth + 3f * arcLength + 2f * straightHeight ? 0f : remainder;
                currentSegmentIndex++;

                continue;
            }

            else
            {
                //Top left corner
                Vector2 origin = new Vector2(rectRadius, straightHeight + rectRadius);

                if(covered < 2f * straightWidth + 2f * straightHeight + 3f * arcLength + step)
                {
                    remainder = covered - (2f * straightWidth + 2f * straightHeight + 3f * arcLength);
                    currentSegmentIndex = 0;
                }

                float numerator = remainder + step * currentSegmentIndex;

                float angle = Mathf.PI - numerator / rectRadius;

                returnPoints[i] = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rectRadius;
                currentSegmentIndex++;
                continue;
            }
        }

        return returnPoints;
    }   

    private void ChangePointColors()
    {
        Vector3 ceiling = userAvatar.transform.position + Vector3.up;

        float dot = Vector3.Dot(userAvatar.transform.forward, Vector3.up);

        int dotSign = dot > 0 ? 1 : dot < 0 ? -1 : 0;

        dot *= dot * dotSign;

        float highestPoint = HighestImage();
        float lowestPoint = LowestImage();

        float conversionFactor = 1f / (highestPoint - lowestPoint);

        for (int i = 0; i < invisibleObjects.Length; i++)
        {
            float height = invisibleObjects[i].transform.position.y;

            float colorInput = (height - lowestPoint) * conversionFactor;

            colorInput += dot;

            images[i].color = new Color((colorInput), colorInput, colorInput, alpha);
        }
    }

    private float HighestImage()
    {
        float highest = float.MinValue;

        for(int i = 0; i < points.Length; i++)
        {
            if (invisibleObjects[i].transform.position.y > highest)
            {
                highest = invisibleObjects[i].transform.position.y;
            }
        }

        return highest;
    }

    private float LowestImage()
    {
        float lowest = float.MaxValue;

        for(int i = 0; i < invisibleObjects.Length; i++)
        {
            if (invisibleObjects[i].transform.position.y < lowest)
            {
                lowest = invisibleObjects[i].transform.position.y;
            }
        }

        return lowest;
    }

    public bool Activated
    {
        get { return activated; }
        set
        {
            for(int i = 0; i < points.Length; i++)
            {
                points[i].SetActive(value);
            }
            activated = value;
        }
    }
}
