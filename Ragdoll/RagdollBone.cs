using Godot;

public partial class RagdollBone : RigidBody3D
{
    [Export] public string BoneName;
    [Export] public Skeleton3D PhysicsSkeleton;

    // [Export] public float Mass = 1.0f;

    public int BoneIndex = 0;

    public override void _Ready()
    {

        if (!IsInstanceValid(PhysicsSkeleton))
            return;

        if (PhysicsSkeleton != null)
        {
            BoneIndex = PhysicsSkeleton.FindBone(BoneName);
        }

        Freeze = false;
        Sleeping = false;
        Mass = Mass;

    }

    public override void _PhysicsProcess(double delta)

    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        Transform3D boneGlobalTransform = PhysicsSkeleton.GetBoneGlobalPose(BoneIndex);

        // Calculate the transform difference between the current global transform of the ragdoll bone and the skeleton bone's global transform
        Transform3D transformDifference = boneGlobalTransform.AffineInverse() * GlobalTransform;

        // Apply the transform difference to the bone's local pose
        Transform3D newBonePose = PhysicsSkeleton.GetBonePose(BoneIndex) * transformDifference;

        // Set the bone's new pose in the skeleton
        PhysicsSkeleton.SetBonePosePosition(BoneIndex, newBonePose.Origin);
        PhysicsSkeleton.SetBonePoseRotation(BoneIndex, newBonePose.Basis.GetRotationQuaternion());
        PhysicsSkeleton.SetBonePoseScale(BoneIndex, newBonePose.Basis.Scale);

    }
}
