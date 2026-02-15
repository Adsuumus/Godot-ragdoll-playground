using Godot;

public partial class RagdollBone : RigidBody3D
{
    [Export] public string BoneName;
    [Export] public Skeleton3D Skeleton;

    public int BoneIndex = 0;

    public override void _PhysicsProcess(double delta)

    {
        if (Engine.IsEditorHint())
        {
            Freeze = true;
            Sleeping = true;
            return;
        }

        if (Skeleton != null)
            BoneIndex = Skeleton.FindBone(BoneName);

        Transform3D boneGlobalTransform = Skeleton.GetBoneGlobalPose(BoneIndex);

        // Calculate the transform difference between the current global transform of the ragdoll bone and the skeleton bone's global transform
        Transform3D transformDifference = boneGlobalTransform.AffineInverse() * GlobalTransform;

        // Apply the transform difference to the bone's local pose
        Transform3D newBonePose = Skeleton.GetBonePose(BoneIndex) * transformDifference;

        // Set the bone's new pose in the skeleton
        Skeleton.SetBonePosePosition(BoneIndex, newBonePose.Origin);
        Skeleton.SetBonePoseRotation(BoneIndex, newBonePose.Basis.GetRotationQuaternion());
        Skeleton.SetBonePoseScale(BoneIndex, newBonePose.Basis.Scale);
    }
}
