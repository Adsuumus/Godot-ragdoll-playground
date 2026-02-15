using Godot;

[Tool]
public partial class CreateRagdoll : Skeleton3D
{
	[ExportToolButton("Generate Ragdoll")]
	public Callable GenerateButton => Callable.From(Generate);

	[Export] public Skeleton3D AnimationSkeleton;
	[Export] public float CapsuleRadius = 0.05f;

	private void Generate()
	{
		Clear();
		CreateBones();
		CreateJoints();
	}

	private void Clear()
	{
		foreach (Node child in GetChildren())
		{
			if (child.Name.ToString().Contains("_rag") || child.Name.ToString().Contains("_col") || child.Name.ToString().Contains("_joint"))
				child.Free();
		}
	}

	private void CreateBones()
	{
		for (int i = 0; i < GetBoneCount(); i++)
		{
			if (GetBoneChildren(i).Length == 0)
				continue;

			var bone = new RagdollBone();
			bone.Name = GetBoneName(i) + "_rag";
			bone.BoneName = GetBoneName(i);
			bone.Skeleton = this;

			bone.CanSleep = false;
			bone.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
			bone.ContinuousCd = true;


			bone.GlobalTransform = GetBoneGlobalPose(i);

			var col = new CollisionShape3D();
			col.Shape = new CapsuleShape3D
			{
				Radius = CapsuleRadius,
				Height = CapsuleRadius * 2f
			};

			col.Name = GetBoneName(i) + "_col";

			AddChild(bone);
			bone.AddChild(col);

			bone.Owner = GetOwner<Node>();
			col.Owner = GetOwner<Node>();
		}
	}

	private RagdollBone FindBody(string name)
	{
		foreach (Node child in GetChildren())
		{
			if (child is RagdollBone rb && rb.Name == name)
				return rb;
		}
		return null;
	}


	private void CreateJoints()
	{
		for (int i = 0; i < GetBoneCount(); i++)
		{
			int parentIndex = GetBoneParent(i);
			if (parentIndex < 0) continue;

			var parentBody = FindBody(GetBoneName(parentIndex) + "_rag");
			var childBody = FindBody(GetBoneName(i) + "_rag");

			if (parentBody == null || childBody == null) continue;

			var joint = new RagdollJoint();
			joint.Name = $"{parentBody.Name}_to_{childBody.Name}_joint".Replace("_rag", "");

			parentBody.AddChild(joint);
			joint.GlobalTransform = GetBoneGlobalPose(i);

			joint.NodeA = joint.GetPathTo(parentBody);
			joint.NodeB = joint.GetPathTo(childBody);

			joint.BoneA = parentBody;
			joint.BoneB = childBody;

			joint.PhysicsSkeleton = this;
			joint.AnimationSkeleton = AnimationSkeleton;

			joint.Owner = GetOwner<Node>();
		}
	}

}
