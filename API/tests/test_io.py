from unittest import TestCase
import oursin as urchin
#pd or npy?

class TestIO(TestCase):
    """Test IO class functions"""

    # def test_sanitize_vector3(self):
    #     """Test sanitize_vector3 function"""
    #     self.assertEqual(urchin.utils.sanitize_vector3([1,2,3]), [1,2,3])
    #     self.assertEqual(urchin.utils.sanitize_vector3((1,2,3)), [1,2,3])
    #     self.assertRaises(Exception, urchin.utils.sanitize_vector3)
    #     self.assertRaises(Exception, urchin.utils.sanitize_vector3, "test")
    #     self.assertRaises(Exception, urchin.utils.sanitize_vector3, ["one fish", "two fish"])
    #     self.assertRaises(Exception, urchin.utils.sanitize_vector3, ["one fish", "two fish", "red fish", "blue fish"])
    #     self.assertEqual(urchin.utils.sanitize_vector3(("cat", "in the", "hat")), ["cat", "in the", "hat"])
    #     self.assertEqual(urchin.utils.sanitize_vector3(["cat", "in the", "hat"]), ["cat", "in the", "hat"])

    def test_upload_file_valid_input(self):
        """Test upload_file function with valid input"""
        ## Tests with incorrect links should be flagged by the sanitize function in utils
        # Test with valid input
        file_name = "test.txt"
        url = "https://drive.google.com/file/d/exampleid123456789/view?usp=sharing"

        # Capture the standard output, so you can check if wget was called
        import io
        from contextlib import redirect_stdout
        with io.StringIO() as captured_output:
            with redirect_stdout(captured_output):
                urchin.io.upload_file(file_name, url)

        # Check if the output contains the wget command
        self.assertIn("wget -O", captured_output.getvalue())

    def test_image(self):
        """Test whether image function gets called"""
        image_url = 'https://picsum.photos/200/300'

        # Import display function from IPython.display to capture its usage
        from IPython.display import Image, display
        original_display = display

        # Create a list to capture the display calls
        display_calls = []

        def custom_display(image_object):
            display_calls.append(image_object)
        
        # Replace the display function with the custom function
        display = custom_display

        # Call the image function
        urchin.io.image(image_url)

        # Restore the original display function
        display = original_display

        # Check if the display function was called with an Image object
        self.assertTrue(display_calls)
        self.assertIsInstance(display_calls[0], Image)
        self.assertEqual(display_calls[0].url, image_url)

    def test_load_df(self):
        csv_url = 'https://drive.google.com/file/d/1Y2gSibo260SIT2AebpTpC5dW2RBNtnlP/view?usp=sharing'
        df = urchin.io.load_df(csv_url)
        self.assertIsInstance(df, pd.DataFrame)
        self.assertEqual(df.shape, (4, 2))

    def test_load_npy(self):
        npy_url = 'https://drive.google.com/file/d/1zE3Vobs5HBH_ne4KOpaPNKZFjhdxl6yQ/view?usp=sharing'
        file_title = 'test.npy'
        array = urchin.io.load_npy(npy_url, file_title)
        self.assertIsInstance(array, np.ndarray)
        self.assertEqual(array.shape, (1, 5))

    
    #downloads use external libraries soooo
