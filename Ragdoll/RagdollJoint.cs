using Godot;

public partial class RagdollJoint : Generic6DofJoint3D
{
	[Export] public Skeleton3D PhysicsSkeleton;
	[Export] public Skeleton3D AnimationSkeleton;

	[Export] public RagdollBone BoneA;
	[Export] public RagdollBone BoneB;

	[Export] public float Stiffness = 1f;
	[Export] public float Damping = 0.5f;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			SetPhysicsProcess(false);
			return;
		}

		SetFlagX(Flag.EnableMotor, false);
		SetFlagY(Flag.EnableMotor, false);
		SetFlagZ(Flag.EnableMotor, false);

		SetParamX(Param.AngularMotorForceLimit, 999999);
		SetParamY(Param.AngularMotorForceLimit, 999999);
		SetParamZ(Param.AngularMotorForceLimit, 999999);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (BoneA == null || BoneB == null)
			return;

		Quaternion animA = AnimationSkeleton
			.GetBoneGlobalPose(BoneA.BoneIndex)
			.Basis.GetRotationQuaternion();

		Quaternion animB = AnimationSkeleton
			.GetBoneGlobalPose(BoneB.BoneIndex)
			.Basis.GetRotationQuaternion();

		Quaternion physA = PhysicsSkeleton
			.GetBoneGlobalPose(BoneA.BoneIndex)
			.Basis.GetRotationQuaternion();

		Quaternion physB = PhysicsSkeleton
			.GetBoneGlobalPose(BoneB.BoneIndex)
			.Basis.GetRotationQuaternion();

		Quaternion animRel = animA.Inverse() * animB;
		Quaternion physRel = physA.Inverse() * physB;

		Quaternion error = physRel.Inverse() * animRel;
		error = error.Normalized();

		Vector3 axis = error.GetAxis();
		float angle = error.GetAngle();

		Vector3 targetVelocity = axis * angle * Stiffness;

		Vector3 currentVelocity =
			BoneB.AngularVelocity - BoneA.AngularVelocity;

		targetVelocity -= currentVelocity * Damping;

		SetParamX(Param.AngularMotorTargetVelocity, targetVelocity.X);
		SetParamY(Param.AngularMotorTargetVelocity, targetVelocity.Y);
		SetParamZ(Param.AngularMotorTargetVelocity, targetVelocity.Z);
	}
}
