using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KochGenerator : MonoBehaviour { 

    protected enum _axis
    {
        XAxis,
        YAxis,
        ZAxis
    }

    protected enum _initiator
    {
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Heptagon,
        Octagon
    };

    public struct LineSegment
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float Length { get; set; }
    }

    [SerializeField]
    protected _axis axis = new _axis();
    [SerializeField]
    protected _initiator initiator = new _initiator();
    [SerializeField]
    protected AnimationCurve _generator;
    protected Keyframe[] _keys;

    [SerializeField]
    protected bool _useBezierCurves;
    [SerializeField]
    [Range(8,24)]
    protected int _bezierVertexCount;

    protected int _generationCount;

    protected int _initiatorPointCount;
    private Vector3[] _initiatorPoint;
    private Vector3 _rotateVector;
    private Vector3 _rotateAxis;
    private float _initialRotation;

    [SerializeField]
    protected float _initiatorSize;

    protected Vector3[] _position;
    protected Vector3[] _targetPosition;
    protected Vector3[] _bezierPosition;
    private List<LineSegment> _lineSegment;

    protected Vector3[] BezierCurve(Vector3[] points, int vertexCount)
    {
        List<Vector3> pointList = new List<Vector3>();
        for(int i = 0; i < points.Length; i += 2)
        {
            if(i+2 <= points.Length - 1)
            {
                for (float ratio = 0f; ratio <= 1f; ratio += 1.0f / vertexCount)
                {
                    var tangentLineVertex1 = Vector3.Lerp(points[i], points[i + 1], ratio);
                    var tangentLineVertex2 = Vector3.Lerp(points[i+1], points[i + 2], ratio);
                    var bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
                    pointList.Add(bezierPoint);
                }
            }
        }
        return pointList.ToArray();
    }

    private void Awake()
    {
        GetInitiatorPoints();
        // assign lists & arrays
        _position = new Vector3[_initiatorPointCount + 1];
        _targetPosition = new Vector3[_initiatorPointCount + 1];
        _lineSegment = new List<LineSegment>();
        _keys = _generator.keys;

        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;
        for (int i = 0; i < _initiatorPointCount; i++)
        {
            _position[i] = _rotateVector * _initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / _initiatorPointCount, _rotateAxis) * _rotateVector;
        }
        _position[_initiatorPointCount] = _position[0];
        _targetPosition = _position;
    }

    protected void KochGenerate(Vector3[] positions, bool outwards, float generatorMultiplier)
    {
        _lineSegment.Clear();
        for(int i = 0; i < positions.Length - 1; i++)
        {
            LineSegment line = new LineSegment();
            line.StartPosition = positions[i];
            if(i == positions.Length - 1)
            {
                line.EndPosition = positions[0];
            }
            else
            {
                line.EndPosition = positions[i + 1];
            }
            line.Direction = (line.EndPosition - line.StartPosition).normalized;
            line.Length = Vector3.Distance(line.EndPosition, line.StartPosition);
            _lineSegment.Add(line);
        }
        // Add the line segment points to a point array
        List<Vector3> newPos = new List<Vector3>();
        List<Vector3> targetPos = new List<Vector3>();

        foreach(LineSegment curLine in _lineSegment)
        {
            newPos.Add(curLine.StartPosition);
            targetPos.Add(curLine.StartPosition);

            for (int j = 1; j < _keys.Length - 1; j++)
            {
                float moveAmount = curLine.Length * _keys[j].time;
                float heightAmount = (curLine.Length * _keys[j].value) * generatorMultiplier;
                Vector3 movePos = curLine.StartPosition + (curLine.Direction * moveAmount);
                Vector3 dir = Quaternion.AngleAxis((outwards ? -90 : 90), _rotateAxis) * curLine.Direction;

                newPos.Add(movePos);
                targetPos.Add(movePos + (dir * heightAmount));

            }
        }
        newPos.Add(_lineSegment[0].StartPosition);
        targetPos.Add(_lineSegment[0].StartPosition);
        _position = new Vector3[newPos.Count];
        _targetPosition = new Vector3[targetPos.Count];
        _position = newPos.ToArray();
        _targetPosition = targetPos.ToArray();
        _bezierPosition = BezierCurve(_targetPosition, _bezierVertexCount);
        _generationCount++;
    }

    private void OnDrawGizmos()
    {
        GetInitiatorPoints();
        _initiatorPoint = new Vector3[_initiatorPointCount];

        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;
        for (int i = 0; i < _initiatorPointCount; i++)
        {
            _initiatorPoint[i] = _rotateVector * _initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / _initiatorPointCount, _rotateAxis) * _rotateVector;
        }
        for (int i = 0; i < _initiatorPointCount; i++)
        {
            Gizmos.color = Color.white;
            Matrix4x4 transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = transformMatrix;
            if (i < _initiatorPointCount - 1)
            {
                Gizmos.DrawLine(_initiatorPoint[i], _initiatorPoint[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(_initiatorPoint[i], _initiatorPoint[0]);
            }
        }
    }

    private void GetInitiatorPoints()
    {
        switch (initiator)
        {
            case _initiator.Triangle:
                _initiatorPointCount = 3;
                _initialRotation = 0;
                break;
            case _initiator.Square:
                _initiatorPointCount = 4;
                _initialRotation = 45;
                break;
            case _initiator.Pentagon:
                _initiatorPointCount = 5;
                _initialRotation = 36;
                break;
            case _initiator.Hexagon:
                _initiatorPointCount = 6;
                _initialRotation = 30;
                break;
            case _initiator.Heptagon:
                _initiatorPointCount = 7;
                _initialRotation = 25.71428f;
                break;
            case _initiator.Octagon:
                _initiatorPointCount = 8;
                _initialRotation = 22.5f;
                break;
            default:
                _initiatorPointCount = 3;
                _initialRotation = 0;
                break;
        };

        switch (axis)
        {
            case _axis.XAxis:
                _rotateVector = new Vector3(1, 0, 0);
                _rotateAxis = new Vector3(0, 0, 1);
                break;
            case _axis.YAxis:
                _rotateVector = new Vector3(0, 1, 0);
                _rotateAxis = new Vector3(1, 0, 0);
                break;
            case _axis.ZAxis:
                _rotateVector = new Vector3(0, 0, 1);
                _rotateAxis = new Vector3(0, 1, 0);
                break;
            default:
                _rotateVector = new Vector3(0, 0, 1);
                _rotateAxis = new Vector3(0, 1, 0);
                break;
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
