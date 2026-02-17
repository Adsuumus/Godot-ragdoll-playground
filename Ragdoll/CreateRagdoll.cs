using Godot;

[Tool]
public partial class CreateRagdoll : Node3D
{
	[ExportToolButton("Generate Ragdoll")]
	public Callable GenerateButton => Callable.From(Generate);

	[Export]
	public Skeleton3D CharacterSkeleton;

	[Export]
	public Skeleton3D ExternalSkeleton;

	[Export]
	public Node3D Point;

	private Node3D ragdoll;

	private void Generate()
	{
		Clear();
		GenerateRagdoll();
	}

	private void Clear()
	{
		foreach (Node child in FindChildren("*Ragdoll*", recursive: true))
		{
			child.Free();
		}
	}

	private Skeleton3D FindSkeleton()
	{
		foreach (Node child in FindChildren("*", recursive: true))
			if (child is Skeleton3D skeleton)
				return skeleton;
		return null;
	}

	private void GenerateRagdoll()
	{
		ragdoll = new Node3D();
		ragdoll.Name = "Ragdoll";
		AddChild(ragdoll);
		ragdoll.Owner = GetOwner<Node>();

		CharacterSkeleton = FindSkeleton();

		// перемещаем кость в глобальные координаты

		Transform3D targetGlobalTransform = Point.GlobalTransform;

		Transform3D localTransform = CharacterSkeleton.GlobalTransform.AffineInverse() * targetGlobalTransform;

		CharacterSkeleton.SetBoneGlobalPose(10, localTransform);

	}

}
