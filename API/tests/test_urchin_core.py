from unittest import TestCase
from unittest.mock import Mock

import oursin as urchin


class TestClass(TestCase):
    """Test demo class functions"""

    # def setUp(self):
    #     """Setup class and mock callback function"""
    #     self.demo = DemoClass('Demo')
    #     self.mock = Mock()

    def test_sanitize_vec3(self):
        self.assertEqual(urchin.utils.sanitize_vector3([1,2,3]), [1,2,3])
        
        self.assertEqual(urchin.utils.sanitize_vector3((1,2,3)), [1,2,3])
        
        self.assertRaises(Exception, urchin.utils.sanitize_vector3, (1,2))