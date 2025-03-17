using System.Linq;
using System.Text;
using UnityEngine;

public class Recording
{
    private readonly AnimationCurve _posXCurve = new AnimationCurve();
    private readonly AnimationCurve _posYCurve = new AnimationCurve();
    private readonly AnimationCurve _rotZCurve = new AnimationCurve();

    public float Duration { get; private set; }

    private readonly Transform _target;

    // Used For Recording
    // 지정된 대상의 위치 및 회전을 기록하는 함수

    public Recording(Transform target)
    {
        _target = target;
    }

    public void AddSnapShot(float elapsed)
    {
        // elapsed = 경과
        Duration = elapsed;

        var pos = _target.position;
        var rot = _target.rotation.eulerAngles;

        UpdateCurve(_posXCurve, elapsed, pos.x);
        UpdateCurve(_posYCurve, elapsed, pos.y);
        UpdateCurve(_rotZCurve, elapsed, rot.z);

        void UpdateCurve(AnimationCurve curve, float time, float value)
        {
            int keyCount = curve.length;
            Keyframe keyframe = new Keyframe(time, value);

            if (IsValueDuplicate(curve,value))
            {
                curve.MoveKey(keyCount - 1, keyframe);
            }
            else
            {
                curve.AddKey(keyframe);
            }
        }

        bool IsValueDuplicate(AnimationCurve curve, float value)
        {
            int keyCount = curve.length;
            if(keyCount > 1)
            {
                Keyframe lastKeyframe = curve.keys[keyCount - 1];
                Keyframe secondLastKeyframe = curve.keys[keyCount - 2];
                
                return Mathf.Approximately(lastKeyframe.value, secondLastKeyframe.value) && Mathf.Approximately(value, lastKeyframe.value);
            }

            return false;
        }
    }

    // Used For Playback 
    // 재생을 위해 사용

    public Pose EvaluatePoint(float elapsed)
    {
        // 위치 및 회전값을 평가하여 Pose 객체 생성 후 반환
        Vector3 position = new Vector3(_posXCurve.Evaluate(elapsed), _posYCurve.Evaluate(elapsed), 0f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, _rotZCurve.Evaluate(elapsed));
        Pose pose = new Pose(position, rotation);
        return pose;
    }

    // Saving and Loading
    // 분석중

    public Recording(string data)
    {
        _target = null;
        Deserialize(data);
        Duration = Mathf.Max(_posXCurve.keys.LastOrDefault().time, _posYCurve.keys.LastOrDefault().time);
    }

    private const char DATA_DELIMITER = '|';
    private const char CURVE_DELIMITER = '\n';

    public string Serialize()
    {
        var builder = new StringBuilder();

        StringifyPoints(_posXCurve);
        StringifyPoints(_posYCurve);
        StringifyPoints(_rotZCurve, false);

        void StringifyPoints(AnimationCurve curve, bool addDelimiter = true)
        {
            for (var i = 0; i < curve.length; i++)
            {
                var point = curve[i];
                builder.Append($"{point.time:F3},{point.value:F2}");
                if (i != curve.length - 1) builder.Append(DATA_DELIMITER);
            }

            if (addDelimiter) builder.Append(CURVE_DELIMITER);
        }

        return builder.ToString();
    }

    private void Deserialize(string data)
    {
        var components = data.Split(CURVE_DELIMITER);

        DeserializePoint(_posXCurve, components[0]);
        DeserializePoint(_posYCurve, components[1]);
        DeserializePoint(_rotZCurve, components[2]);

        void DeserializePoint(AnimationCurve curve, string d)
        {
            var splitValues = d.Split(DATA_DELIMITER);
            foreach (var timeValPair in splitValues)
            {
                var s = timeValPair.Split(',');
                var kf = new Keyframe(float.Parse(s[0]), float.Parse(s[1]));
                curve.AddKey(kf);
            }
        }
    }
}