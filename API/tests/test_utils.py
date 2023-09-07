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




