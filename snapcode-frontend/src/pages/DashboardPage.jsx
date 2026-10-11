import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Navbar from '../components/Navbar';
import ProjectCard from '../components/ProjectCard';
import CreateProjectModal from '../components/CreateProjectModal';
import { api } from '../services/api';
import './DashboardPage.css';

export default function DashboardPage() {
  const navigate = useNavigate();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    try {
      setLoading(true);
      const data = await api.getProjects();
      console.log('Projects API response:', data);
      
      let projectsArray = [];
      if (Array.isArray(data)) {
        projectsArray = data;
      } else if (data && Array.isArray(data.projects)) {
        projectsArray = data.projects;
      } else if (data && Array.isArray(data.results)) {
        projectsArray = data.results;
      }
      
      setProjects(projectsArray);
    } catch (error) {
      console.error('Failed to load projects:', error);
      setProjects([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateProject = async (projectData) => {
    try {
      // Create empty main.dart AST by default
      const defaultAst = {
        'lib/main.dart': { metadata: { total_block_count: 0 }, blocks: { blocks: [] } }
      };
      
      const newProject = await api.createProject({
        ...projectData,
        blockly_ast: JSON.stringify(defaultAst)
      });
      setIsCreateModalOpen(false);
      navigate(`/editor/${newProject.id}`);
    } catch (error) {
      console.error('Failed to create project:', error);
      alert('Failed to create project');
    }
  };

  const handleDeleteProject = async (id) => {
    if (window.confirm('Are you sure you want to delete this project?')) {
      try {
        await api.deleteProject(id);
        await loadProjects();
      } catch (error) {
        console.error('Failed to delete project:', error);
      }
    }
  };

  const loadSampleProjects = async () => {
    try {
      setLoading(true);
      
      const sampleAst = {
        'lib/main.dart': {
          metadata: { total_block_count: 2 },
          blocks: {
            blocks: [
              {
                type: 'flutter_app_scaffold',
                id: 'scaffold_1',
                x: 20,
                y: 20,
                fields: { APP_TITLE: 'Sample Counter', THEME_COLOR: '#7c3aed' },
                inputs: {
                  BODY: {
                    block: {
                      type: 'flutter_center',
                      id: 'center_1',
                      inputs: {
                        CHILD: {
                          block: {
                            type: 'flutter_text',
                            id: 'text_1',
                            fields: { TEXT_VALUE: 'Counter: 0', FONT_SIZE: 24, FONT_WEIGHT: 'FontWeight.bold', TEXT_COLOR: '#ffffff' }
                          }
                        }
                      }
                    }
                  }
                }
              }
            ]
          }
        }
      };

      await api.createProject({
        name: 'Sample: Counter App',
        description: 'A basic counter app layout',
        target_platforms: ['mobile', 'web'],
        blockly_ast: JSON.stringify(sampleAst)
      });
      await loadProjects();
    } catch (error) {
      console.error('Failed to load sample projects:', error);
    }
  };

  const filteredProjects = projects.filter(p => 
    p.name && p.name.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      
      <main className="p-xl w-full flex-1" style={{ maxWidth: '1280px', margin: '0 auto' }}>
        <div className="flex justify-between items-center mb-lg">
          <div>
            <h1 className="text-3xl font-bold mb-xs">My Projects</h1>
            <p className="text-secondary">Manage your visual Flutter apps</p>
          </div>
          <div className="flex gap-md">
            <button className="btn btn-secondary" onClick={loadSampleProjects} disabled={loading}>
              Load Samples
            </button>
            <button className="btn btn-primary" onClick={() => setIsCreateModalOpen(true)}>
              New Project
            </button>
          </div>
        </div>

        <div className="input-group mb-xl" style={{ maxWidth: '400px' }}>
          <input
            type="text"
            className="input-field"
            placeholder="Search projects..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        {loading ? (
          <div className="flex justify-center p-xl">
            <div className="spinner spinner-lg"></div>
          </div>
        ) : filteredProjects.length > 0 ? (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '24px' }}>
            {filteredProjects.map(project => (
              <ProjectCard 
                key={project.id} 
                project={project} 
                onDelete={() => handleDeleteProject(project.id)}
              />
            ))}
          </div>
        ) : (
          <div className="glass-card p-xl text-center">
            <h3 className="text-xl font-semibold mb-sm">No projects found</h3>
            <p className="text-secondary mb-md">
              {search ? "Try a different search term" : "Create your first project to get started"}
            </p>
            {!search && (
              <button className="btn btn-primary" onClick={() => setIsCreateModalOpen(true)}>
                New Project
              </button>
            )}
          </div>
        )}
      </main>

      {isCreateModalOpen && (
        <CreateProjectModal 
          onClose={() => setIsCreateModalOpen(false)}
          onCreate={handleCreateProject}
        />
      )}
    </div>
  );
}
