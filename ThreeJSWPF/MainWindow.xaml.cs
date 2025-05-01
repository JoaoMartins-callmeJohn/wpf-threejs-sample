using System.Windows;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace ThreeJSWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			InitializeAsync();
		}

		async void InitializeAsync()
		{
			// Set up the WebView2 environment
			var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfWebView2ThreeJs");
			var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
			await webView.EnsureCoreWebView2Async(env);

			// Set up communication between C# and JavaScript
			webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

			// Create HTML file with THREE.js scene
			string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "threejs-scene.html");
			File.WriteAllText(htmlFilePath, GetThreeJsHtml());

			// Navigate to the HTML file
			webView.CoreWebView2.Navigate(new Uri(htmlFilePath).AbsoluteUri);
		}

		private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			// Handle messages from JavaScript
			string message = e.WebMessageAsJson;
			MessageBox.Show($"Message from JavaScript: {message}");
		}
		private void btnBoxes_Click(object sender, RoutedEventArgs e)
		{
			string script = "changeGeometry('box');";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private void btnSpheres_Click(object sender, RoutedEventArgs e)
		{
			string script = "changeGeometry('sphere');";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private void btnCylinders_Click(object sender, RoutedEventArgs e)
		{
			string script = "changeGeometry('cylinder');";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private void btnTorus_Click(object sender, RoutedEventArgs e)
		{
			string script = "changeGeometry('torus');";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private void btnRandomSize_Click(object sender, RoutedEventArgs e)
		{
			string script = "randomizeSize();";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private void btnRandomColor_Click(object sender, RoutedEventArgs e)
		{
			string script = "randomizeColors();";
			webView.CoreWebView2.ExecuteScriptAsync(script);
		}

		private string GetThreeJsHtml()
		{
			return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>THREE.js Scene in WebView2</title>
    <style>
        body { margin: 0; overflow: hidden; }
        canvas { display: block; }
    </style>
</head>
<body>
    <script src='https://unpkg.com/three@0.128.0/build/three.min.js'></script>
    <!-- Use unpkg CDN as an alternative -->
    <script src='https://unpkg.com/three@0.128.0/examples/js/controls/OrbitControls.js'></script>
    <script>
        // Initialize the scene, camera, and renderer
        const scene = new THREE.Scene();
        scene.background = new THREE.Color(0x222222);
        
        const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        camera.position.z = 5;
        
        const renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
        document.body.appendChild(renderer.domElement);
        
        // Add OrbitControls to the camera
        const controls = new THREE.OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true; // Add smooth damping effect
        controls.dampingFactor = 0.05;
        controls.rotateSpeed = 0.8; // Adjust rotation speed
        controls.zoomSpeed = 1.2; // Adjust zoom speed
        controls.panSpeed = 0.8; // Adjust pan speed
        controls.minDistance = 2; // Minimum zoom distance
        controls.maxDistance = 20; // Maximum zoom distance
        controls.maxPolarAngle = Math.PI * 0.9; // Prevent going below the ground plane
        
        // Add a light
        const light = new THREE.DirectionalLight(0xffffff, 1);
        light.position.set(1, 1, 1).normalize();
        scene.add(light);
        
        // Add ambient light
        const ambientLight = new THREE.AmbientLight(0x404040);
        scene.add(ambientLight);
        
        // Add a grid helper for reference
        const gridHelper = new THREE.GridHelper(10, 10);
        scene.add(gridHelper);
        
        // Create materials with different colors
        const materials = [
            new THREE.MeshPhongMaterial({ color: 0xff0000 }), // red
            new THREE.MeshPhongMaterial({ color: 0x00ff00 }), // green
            new THREE.MeshPhongMaterial({ color: 0x0000ff }), // blue
            new THREE.MeshPhongMaterial({ color: 0xffff00 }), // yellow
            new THREE.MeshPhongMaterial({ color: 0xff00ff })  // purple
        ];
        
        // Store objects and their current geometries
        const objects = [];
        let currentGeometryType = 'box';
        
        // Define all geometry types
        const geometryTypes = {
            'box': () => new THREE.BoxGeometry(1, 1, 1),
            'sphere': () => new THREE.SphereGeometry(0.6, 16, 16),
            'cylinder': () => new THREE.CylinderGeometry(0.5, 0.5, 1, 16),
            'torus': () => new THREE.TorusGeometry(0.5, 0.2, 16, 32)
        };
        
        // Create objects
        function createObjects() {
            // Clear previous objects
            objects.forEach(obj => scene.remove(obj));
            objects.length = 0;
            
            // Create new objects with current geometry type
            const geometry = geometryTypes[currentGeometryType]();
            
            for (let i = 0; i < 5; i++) {
                const object = new THREE.Mesh(geometry, materials[i]);
                object.position.x = (i - 2) * 1.5;
                scene.add(object);
                objects.push(object);
            }
        }
        
        // Change geometry type
        function changeGeometry(type) {
            if (geometryTypes[type]) {
                currentGeometryType = type;
                createObjects();
                
                // Send message back to C# if needed
                window.chrome.webview.postMessage(`Geometry changed to ${type}`);
            }
        }
        
        // Randomize the size of objects
        function randomizeSize() {
            objects.forEach(obj => {
                const scale = 0.5 + Math.random() * 1.5;
                obj.scale.set(scale, scale, scale);
            });
            
            // Send message back to C#
            window.chrome.webview.postMessage('Sizes randomized');
        }
        
        // Randomize the colors of objects
        function randomizeColors() {
            objects.forEach(obj => {
                obj.material.color.setHex(Math.random() * 0xffffff);
            });
            
            // Send message back to C#
            window.chrome.webview.postMessage('Colors randomized');
        }
        
        // Reset camera to default position
        function resetCamera() {
            // Create a smooth animation to the default camera position
            const startPosition = camera.position.clone();
            const startTarget = controls.target.clone();
            
            // Define the default camera position and target
            const defaultPosition = new THREE.Vector3(0, 0, 5);
            const defaultTarget = new THREE.Vector3(0, 0, 0);
            
            // Animate over 1 second (60 frames at 60fps)
            const frames = 60;
            let frame = 0;
            
            function animateReset() {
                if (frame < frames) {
                    // Calculate interpolation factor (ease out)
                    const t = 1 - Math.pow(1 - frame / frames, 3); // Cubic ease out
                    
                    // Interpolate camera position
                    camera.position.lerpVectors(startPosition, defaultPosition, t);
                    
                    // Interpolate target position
                    controls.target.lerpVectors(startTarget, defaultTarget, t);
                    
                    // Update controls
                    controls.update();
                    
                    // Next frame
                    frame++;
                    requestAnimationFrame(animateReset);
                } else {
                    // Ensure we reach exactly the desired position
                    camera.position.copy(defaultPosition);
                    controls.target.copy(defaultTarget);
                    controls.update();
                    
                    // Notify C# that camera reset is complete
                    window.chrome.webview.postMessage('Camera reset complete');
                }
            }
            
            // Start animation
            animateReset();
        }
        
        // Animation loop
        function animate() {
            requestAnimationFrame(animate);
            
            // Update orbit controls - critical for smooth damping
            controls.update();
            
            // Gently rotate the objects - slower than before to allow manual control
            objects.forEach((obj, index) => {
                obj.rotation.x += 0.002 * (index + 1);
                obj.rotation.y += 0.002 * (index + 1);
            });
            
            renderer.render(scene, camera);
        }
        
        // Handle window resize
        window.addEventListener('resize', () => {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        });
        
        // Initialize objects and start animation
        createObjects();
        animate();
    </script>
</body>
</html>";
		}

	}
}