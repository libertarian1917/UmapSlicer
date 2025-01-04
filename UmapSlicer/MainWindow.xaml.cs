using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.IO;
using System.Diagnostics;
using UmapSlicer.Enums;
using UmapSlicer.Interaction;
using System.Transactions;


namespace UmapSlicer
{
    /// <summary>
    /// Umap view logic
    /// </summary>
    public partial class MainWindow : Window
    {
        // Moving object
        private Point lastMousePosition;
        private ArrowHandler arrowHandler;
        private bool secondCheck = false;

        // Scene
        private  Dictionary<Enums.Materials, DiffuseMaterial> ColorDict = new Dictionary<Enums.Materials, DiffuseMaterial>();
        private Model3DGroup models;
        private List<ModelVisual3D> sceneDetails;

        // Objects
        ModelVisual3D hoveredObject;
        List<ModelVisual3D> objectsOutOfScene = new List<ModelVisual3D>();



        public MainWindow()
        {
            InitializeComponent();
            Debug.WriteLine("Test");

            InitializeColors();
            InitializeScene();
        }

        private void ChangeMaterial(ModelVisual3D model, Enums.Materials material)
        {
            if (model.Content is GeometryModel3D geometry)
            {
                geometry.Material = ColorDict[material];
            }
        }

        private ModelVisual3D RenderFileSTL(string filePath, Enums.Materials material = Enums.Materials.ModelDefault)
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Нету STL? А если найду?");
                return null;
            }
            // Creating StLReader for loading of STL file
            var stlReader = new StLReader();
            models = stlReader.Read(filePath);

            foreach (GeometryModel3D geometryModel in models.Children)
            {
                geometryModel.Material = ColorDict[material];
                geometryModel.BackMaterial = ColorDict[material];
            }
            // Add models to MainViewport3D
            var model = new ModelVisual3D { Content = models };
            MainViewport.Children.Add(model);
            // Potential access to the added model
            return model;
        }

        private ModelVisual3D GetModelUnderMouse(MouseEventArgs e, HelixViewport3D viewport)
        {
            var hitResult = VisualTreeHelper.HitTest(viewport, e.GetPosition(viewport)) as RayHitTestResult;
            if (hitResult == null) return null;

            if (hitResult != null)
            {
                foreach (ModelVisual3D model in sceneDetails)
                {
                    if (model == hitResult.VisualHit) return null;
                }
                if (hitResult.VisualHit is ModelVisual3D modelVisual)
                {
                    return modelVisual;
                }
            }

            return null;
        }

        
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = e.GetPosition(MainViewport);

            var modelMV = GetModelUnderMouse(e, MainViewport);
            var modelOV = GetModelUnderMouse(e, OverlayViewport);

            if (modelMV != null && modelOV == null)
            {
                arrowHandler.DetectModel(modelMV);
            }
            else if(modelMV != null && modelOV != null)
            {
                arrowHandler.DetectModel(modelOV);
            }
            else if(modelMV == null && modelOV != null)
            {
                arrowHandler.DetectModel(modelOV);
            }
            else
            {
                arrowHandler.DeselectModel();
            }
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (arrowHandler.SelectedModel != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(MainViewport);
                arrowHandler.UpdateDragging(mousePosition, lastMousePosition);
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // If arrow is dragging - release dragging
            if (arrowHandler.DraggedArrow != Axis.NoAxis && arrowHandler.IsDragging)
            {
                arrowHandler.StopDragging();
                return;
            }
        }

        private void Viewport_CameraChanged(object sender, RoutedEventArgs e)
        {
            // Sync cameras
            overlayCameraView.Position = mainCameraView.Position;
            overlayCameraView.LookDirection = mainCameraView.LookDirection;
            overlayCameraView.UpDirection = mainCameraView.UpDirection;
            if (mainCameraView is PerspectiveCamera perspectiveCamera1 && overlayCameraView is PerspectiveCamera perspectiveCamera2)
            {
                perspectiveCamera2.FieldOfView = perspectiveCamera1.FieldOfView;
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "Database files (*.stl)|*.stl";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true) return;

            string selectedFileName = openFileDialog.FileName;
            if (selectedFileName == "") return;

            RenderFileSTL(selectedFileName);
        }

        private void MenuCleanScene_Click(object sender, RoutedEventArgs e)
        {

        }
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CleanScene()
        {
            
        }

        /// <summary>
        /// Define all default object for scene and added to <see cref="sceneDetails">sceneDetails</see>
        /// </summary>
        private void InitializeScene()
        {
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddBox(new Point3D(0, 0, -0.0501), 25, 21, 0.1);
            var geometry = meshBuilder.ToMesh();
            var model = new GeometryModel3D(geometry, ColorDict[Enums.Materials.PlateDefault]);
            model.BackMaterial = ColorDict[Enums.Materials.PlateDefault];
            ModelVisual3D plate = new ModelVisual3D { Content = model };
            MainViewport.Children.Add(plate);

            sceneDetails = new List<ModelVisual3D>() { gridLines, frontLine, leftLine, backLine, rightLine, plate };
            arrowHandler = new ArrowHandler(OverlayViewport);

            var brush = Brushes.DimGray;
            MainViewport.Background = brush;
        }

        private void InitializeColors()
        {
            DiffuseMaterial redMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)215, (byte)0, (byte)0)));
            DiffuseMaterial grayDefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)222, (byte)222, (byte)222)));
            DiffuseMaterial blueGreenMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)40, (byte)170, (byte)140)));
            DiffuseMaterial greenMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)115, (byte)210, (byte)50)));
            DiffuseMaterial darkGrayMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)45, (byte)45, (byte)45)));

            ColorDict.Add(Enums.Materials.OutOfBorders, redMaterial);
            ColorDict.Add(Enums.Materials.ModelDefault, grayDefaultMaterial);
            ColorDict.Add(Enums.Materials.OnModelHover, blueGreenMaterial);
            ColorDict.Add(Enums.Materials.ModelSelected, greenMaterial);
            ColorDict.Add(Enums.Materials.PlateDefault, darkGrayMaterial);
        }

    }
}