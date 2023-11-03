from unittest import TestCase
import oursin as urchin


class TestUtils(TestCase):
    """Test utils class functions"""
    
    # def test_vector_sum(self):
    #     """Test vector_sum function"""
    #     self.assertEqual(self.demo.vector_sum((2, 2, 2), (2, 2, 2)), (4, 4, 4))
    #     self.assertEqual(self.demo.vector_sum((2, 2, 2), (0, 0, 0)), (2, 2, 2))
    #     self.assertEqual(self.demo.vector_sum((1, 2, 3), (4, 5, 6)), (5, 7, 9))

    #     self.assertRaises(TypeError, self.demo.vector_sum, ("hi", "this", "is"), ("a", "test", "case"))

    def test_sanitize_vector3(self):
        """Test sanitize_vector3 function"""
        self.assertEqual(urchin.utils.sanitize_vector3([1,2,3]), [1,2,3])
        self.assertEqual(urchin.utils.sanitize_vector3((1,2,3)), [1,2,3])
        self.assertRaises(Exception, urchin.utils.sanitize_vector3)
        self.assertRaises(Exception, urchin.utils.sanitize_vector3, "test")
        self.assertRaises(Exception, urchin.utils.sanitize_vector3, ["one fish", "two fish"])
        self.assertRaises(Exception, urchin.utils.sanitize_vector3, ["one fish", "two fish", "red fish", "blue fish"])
        self.assertEqual(urchin.utils.sanitize_vector3(("cat", "in the", "hat")), ["cat", "in the", "hat"])
        self.assertEqual(urchin.utils.sanitize_vector3(["cat", "in the", "hat"]), ["cat", "in the", "hat"])
    
    def test_sanitize_colors(self):
        """Test sanitize_colors function"""
        self.assertEqual(urchin.utils.sanitize_color('#FFA500'), '#FFA500')
        self.assertEqual(urchin.utils.sanitize_color('00FF00'), '#00FF00')
        self.assertEqual(urchin.utils.sanitize_color([0.5, 0.2, 0.8]), '#7F33CC')
        self.assertEqual(urchin.utils.sanitize_color([128, 64, 255]), '#8040FF')
        self.assertRaises(Exception,urchin.utils.sanitize_color([300, 50, 100]))
        self.assertRaises(Exception,urchin.utils.sanitize_color(123))
            
    def test_sanitize_float(self):
        """Test sanitize_float function"""
        self.assertEqual(urchin.utils.sanitize_float(3.14), 3.14)
        self.assertEqual(urchin.utils.sanitize_float("2.718"), 2.718)
        self.assertRaises(Exception,urchin.utils.sanitize_float("invalid"))
        self.assertRaises(Exception, urchin.utils.sanitize_float([1, 2, 3]))
        self.assertRaises(Exception, urchin.utils.sanitize_float({'value': 42}))
        self.assertRaises(Exception, urchin.utils.sanitize_float(None))
        self.assertRaises(Exception, urchin.utils.sanitize_float(True))

    ## material?

    def test_sanitize_lists(self):
        """Test sanitize_lists function"""
        self.assertEqual(urchin.utils.sanitize_list([1, 2, 3, 4]), [1, 2, 3, 4])
        self.assertEqual(urchin.utils.sanitize_list(42, length=3), [42, 42, 42])
        self.assertRaises(Exception,urchin.utils.sanitize_list(3.14))
        self.assertRaises(Exception,urchin.utils.sanitize_list("string"))
            
    def test_sanitize_string(self):
        """Test sanitize_string function"""
        self.assertEqual(urchin.utils.sanitize_string("string"), "string")
        self.assertEqual(urchin.utils.sanitize_string(""), "")
        self.assertRaises(Exception,urchin.utils.sanitize_string([1, 2, 3]))
        self.assertRaises(Exception, urchin.utils.sanitize_string({'value': 42}))
        self.assertRaises(Exception, urchin.utils.sanitize_string(None))
        self.assertRaises(Exception, urchin.utils.sanitize_string(True))    


    def test_sanitize_drive_url(self):
        """Test sanitize_drive_url function"""
        self.assertEqual(urchin.utils.sanitize_drive_url("https://drive.google.com/file/d/1Vn5OpFRkEu_GYSmi9kZYXH8WlwmJ6Qs6/view?usp=sharing"), "https://drive.google.com/uc?id=1Vn5OpFRkEu_GYSmi9kZYXH8WlwmJ6Qs6") #all probes csv
        self.assertEqual(urchin.utils.sanitize_drive_url("https://drive.google.com/file/d/1zE3Vobs5HBH_ne4KOpaPNKZFjhdxl6yQ/view?usp=sharing"), "https://drive.google.com/uc?id=1zE3Vobs5HBH_ne4KOpaPNKZFjhdxl6yQ") #test array npy
        self.assertRaises(Exception, urchin.utils.sanitize_drive_url)
        self.assertRaises(Exception, urchin.utils.sanitize_drive_url, "test")
        self.assertRaises(Exception, urchin.utils.sanitize_drive_url,"https://drive.google.com/drive/folders/1vaHEXjG0M2jjlMceSvZhbOFCer4cdbR4")
        
    def test_sanitize_side(self):
        """Test sanitize_side function"""
        self.assertEqual(urchin.utils.sanitize_side("VISp", "full"), "VISp")
        self.assertEqual(urchin.utils.sanitize_side("VISp", "left"), "VISp-lh")
        self.assertEqual(urchin.utils.sanitize_side("VISp", "right"), "VISp-rh")
        self.assertRaises(Exception,urchin.utils.sanitize_side("VISp", "invalid"))