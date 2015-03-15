cd docs_src\mkdocs-site
mkdocs build --clean
cd ..\..\
cd docs_src\doxygen
doxygen.exe doxygen-config
