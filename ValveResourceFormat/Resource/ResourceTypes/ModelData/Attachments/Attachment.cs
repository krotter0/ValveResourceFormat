using System.Collections;
using System.Linq;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Serialization.KeyValues;

namespace ValveResourceFormat.ResourceTypes.ModelData.Attachments
{
    public class Attachment : IEnumerable<Attachment.Influence>
    {
        public readonly struct Influence
        {
            public string Name { get; init; }
            public Vector3 Offset { get; init; }
            public Quaternion Rotation { get; init; }
            public double Weight { get; init; }
        }

        public string Name { get; init; }
        public bool IgnoreRotation { get; init; }

        [System.Runtime.CompilerServices.InlineArray(3)]
        public struct Influence3
        {
            private Influence influence;
        }
        public Influence3 Influences;

        public int Length { get; init; }

        public Attachment(KVObject attachmentData)
        {
            var valueData = attachmentData.GetSubCollection("value");

            Name = valueData.GetStringProperty("m_name");
            IgnoreRotation = valueData.GetProperty<bool>("m_bIgnoreRotation");

            var influenceNames = valueData.GetArray<string>("m_influenceNames");
            var influenceRotations = valueData.GetArray("m_vInfluenceRotations").Select(v => v.ToQuaternion()).ToArray();
            var influenceOffsets = valueData.GetArray("m_vInfluenceOffsets", v => v.ToVector3());
            var influenceWeights = valueData.GetArray<double>("m_influenceWeights");

            Length = valueData.GetInt32Property("m_nInfluences");

            for (var i = 0; i < Length; i++)
            {
                Influences[i] = new Influence
                {
                    Name = influenceNames[i],
                    Rotation = influenceRotations[i],
                    Offset = influenceOffsets[i],
                    Weight = influenceWeights[i]
                };
            }
        }

        public IEnumerator<Influence> GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return Influences[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
