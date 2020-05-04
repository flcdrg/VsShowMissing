#include "stdafx.h"
#include "testclass.h"

void TestClass::testPrint()
{
    std::cout << m_sVal;
}

TestClass::TestClass(std::string sVal)
{
    m_sVal = sVal;
}
