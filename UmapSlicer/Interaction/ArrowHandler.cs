using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using UmapSlicer.Enums;

namespace UmapSlicer.Interaction
{
    /// <summary>
    /// All the interaction with an object in Umap is implemented here 
    /// </summary>
    public class ArrowHandler
    {
        private HelixViewport3D overlayViewport;
        private ModelVisual3D selectedModel;
        private Axis draggedArrow = Axis.NoAxis; // Axis.NoAxis for non-selected state

        public ModelVisual3D SelectedModel { get { return selectedModel; } }
        public Axis DraggedArrow { get { return draggedArrow; } }

        double offsetX, offsetY, offsetZ;

        // Arrows as cones and arrow lines for axises
        private LinesVisual3D xArrow, yArrow, zArrow;
        private ModelVisual3D xArrowCone, yArrowCone, zArrowCone;

        // Flags
        public bool AreArrowsVisible = false;
        public bool IsDragging = false;

        public ArrowHandler(HelixViewport3D overlayViewport)
        {
            this.overlayViewport = overlayViewport;
        }

        private void CreateArrows()
        {
            if (selectedModel == null) return;

            var bounds = selectedModel.Content.Bounds;
            var transform = selectedModel.Transform as TranslateTransform3D ?? new TranslateTransform3D();

            Point3D center = new Point3D(
                bounds.X + bounds.SizeX / 2,
                bounds.Y + bounds.SizeY / 2,
                bounds.Z + bounds.SizeZ / 2);

            // Создание стрелок
            xArrow = CreateArrow(Colors.Red, center, new Vector3D(1, 0, 0), bounds.SizeX * 0.7);
            yArrow = CreateArrow(Colors.Green, center, new Vector3D(0, 1, 0), bounds.SizeY * 0.7);
            zArrow = CreateArrow(Colors.Blue, center, new Vector3D(0, 0, 1), bounds.SizeZ * 0.7);

            // Создание конусов
            xArrowCone = CreateCone(
                new Point3D(center.X + bounds.SizeX * 0.7, center.Y, center.Z),
                new Point3D(center.X + bounds.SizeX * 0.7 + 0.6, center.Y, center.Z),
                radius: 0.15, segments: 27, color: Colors.Red);

            yArrowCone = CreateCone(
                new Point3D(center.X, center.Y + bounds.SizeY * 0.7, center.Z),
                new Point3D(center.X, center.Y + bounds.SizeY * 0.7 + 0.6, center.Z),
                radius: 0.15, segments: 27, color: Colors.Green);

            zArrowCone = CreateCone(
                new Point3D(center.X, center.Y, center.Z + bounds.SizeZ * 0.7),
                new Point3D(center.X, center.Y, center.Z + bounds.SizeZ * 0.7 + 0.6),
                radius: 0.15, segments: 27, color: Colors.Blue);
        }

        private ModelVisual3D CreateCone(Point3D baseCenter, Point3D topPoint, double radius, int segments, Color color)
        {
            var meshBuilder = new MeshBuilder();
            // Add Cone
            meshBuilder.AddCone(baseCenter, topPoint, radius, true, segments);
            // Turn to mesh
            var mesh = meshBuilder.ToMesh();
            var material = MaterialHelper.CreateMaterial(color);
            // Create model
            var geometryModel = new GeometryModel3D(mesh, material);
            return new ModelVisual3D { Content = geometryModel };
        }

        private LinesVisual3D CreateArrow(Color color, Point3D startPoint, Vector3D direction, double arrowLength)
        {
            var arrow = new LinesVisual3D
            {
                Color = color,
                Thickness = 2,
            };
            // Add two points to empty arrow
            arrow.Points.Add(startPoint);
            arrow.Points.Add(new Point3D(direction.X * arrowLength, direction.Y * arrowLength, direction.Z * arrowLength));
            return arrow;
        }

        public void StopDragging()
        {
            // End dragging mode
            IsDragging = false;
            draggedArrow = Axis.NoAxis;
            overlayViewport.ReleaseMouseCapture();
        }

        /// <summary>
        /// Moving <see cref="selectedModel">selectedModel</see> with mouse coordinates
        /// </summary>
        /// <param name="currentMousePosition"></param>
        /// <param name="lastMousePosition"></param>
        public void UpdateDragging(Point currentMousePosition, Point lastMousePosition)
        {
            if (!IsDragging || draggedArrow == Axis.NoAxis || selectedModel == null) return;
            // Я хочу пиццу и вареники
            Vector delta = currentMousePosition - lastMousePosition;

            var transform = selectedModel.Transform as TranslateTransform3D ?? new TranslateTransform3D();
            var camera = overlayViewport.Camera as PerspectiveCamera;
            var lookDirection = camera.LookDirection;
            var upDirection = camera.UpDirection;
            lookDirection.Normalize();
            upDirection.Normalize();
            double scaleFactor = 0.02;

            if (draggedArrow == Axis.X)
            {
                transform.OffsetX = offsetX + delta.X * getSign(lookDirection.Y * upDirection.Z) * scaleFactor;
            }
            else if (draggedArrow == Axis.Y)
            {
                transform.OffsetY = offsetY + delta.X * getSign(-lookDirection.X * upDirection.Z) * scaleFactor;
            }
            else if (draggedArrow == Axis.Z)
            {
                transform.OffsetZ = offsetZ + delta.Y * -getSign(upDirection.Z) * scaleFactor;
            }
            selectedModel.Transform = transform;
            UpdateArrowPositions();
        }

        /// <summary>
        /// The same as <see cref="Math.Abs">Math.Abs</see> but returns either 1 or -1
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private double getSign(double num)
        {
            double result = Math.Sign(num);
            if (result == 0) return 1;
            return result;
        }

        /// <summary>
        /// Moving arrows following <see cref="selectedModel">selectedModel</see>
        /// </summary>
        private void UpdateArrowPositions()
        {
            if (selectedModel == null || !AreArrowsVisible) return;

            var transform = selectedModel.Transform as TranslateTransform3D ?? new TranslateTransform3D();
            var bounds = selectedModel.Content.Bounds;

            Point3D center = new Point3D(
                bounds.X + bounds.SizeX / 2 + transform.OffsetX,
                bounds.Y + bounds.SizeY / 2 + transform.OffsetY,
                bounds.Z + bounds.SizeZ / 2 + transform.OffsetZ);

            xArrow.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
            yArrow.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
            zArrow.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);

            xArrowCone.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
            yArrowCone.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
            zArrowCone.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
        }

        /// <summary>
        /// Identify the triggered object
        /// </summary>
        /// <param name="modelVisual"></param>
        public void DetectModel(ModelVisual3D modelVisual)
        {
            if (selectedModel == modelVisual)
            {
                return;
            }
            else if (modelVisual == xArrow || modelVisual == xArrowCone)
            {
                draggedArrow = Axis.X;
            }
            else if (modelVisual == yArrow || modelVisual == yArrowCone)
            {
                draggedArrow = Axis.Y;
            }
            else if (modelVisual == zArrow || modelVisual == zArrowCone)
            {
                draggedArrow = Axis.Z;
            }
            else
            {
                if (selectedModel != null) DeselectModel();
                SelectModel(modelVisual);
                return;
            }

            var transform = selectedModel.Transform as TranslateTransform3D ?? new TranslateTransform3D();
            offsetX = transform.OffsetX;
            offsetY = transform.OffsetY;
            offsetZ = transform.OffsetZ;
            IsDragging = true;
            overlayViewport.CaptureMouse();
        }

        public void SelectModel(ModelVisual3D model)
        {
            selectedModel = model;
            CreateArrows();
            ShowArrows();
            UpdateArrowPositions();
        }

        public void DeselectModel()
        {
            selectedModel = null;
            HideArrows();
        }

        private void ShowArrows()
        {
            if (!AreArrowsVisible)
            {
                overlayViewport.Children.Add(xArrow);
                overlayViewport.Children.Add(yArrow);
                overlayViewport.Children.Add(zArrow);
                overlayViewport.Children.Add(xArrowCone);
                overlayViewport.Children.Add(yArrowCone);
                overlayViewport.Children.Add(zArrowCone);
                AreArrowsVisible = true;
            }
        }

        private void HideArrows()
        {
            if (AreArrowsVisible)
            {
                overlayViewport.Children.Remove(xArrow);
                overlayViewport.Children.Remove(yArrow);
                overlayViewport.Children.Remove(zArrow);
                overlayViewport.Children.Remove(xArrowCone);
                overlayViewport.Children.Remove(yArrowCone);
                overlayViewport.Children.Remove(zArrowCone);
                AreArrowsVisible = false;
            }
        }
    }
}
